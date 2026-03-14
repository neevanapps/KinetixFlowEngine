using KinetixFlowEngine.Core.Bootstrap;
using KinetixFlowEngine.Core.Config;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Models;
using KinetixFlowEngine.Core.Persistence;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Options;
using Serilog;
using System.Collections.Concurrent;

namespace KinetixFlowEngine.Core
{
    public class Worker : BackgroundService
    {
        private readonly ITradeStreamClient _tradeStreamClient;
        private readonly FlowTradeBuffer _flowTradeBuffer;
        private readonly ILogger<Worker> _logger;
        private readonly KinetixEngineProcessor _engineProcessor;
        private readonly EngineWarmupManager _warmup;
        private readonly PriceTrendEngine _priceEngine;
        private readonly ScoreTrendEngine _scoreEngine;
        private readonly ProbabilityTrendEngine _probEngine;
        private readonly EngineBootstrapService _bootstrap;
        private readonly TelegramService _telegram;
        private readonly ExceptionAlertAggregator _exceptionAggregator;

        private readonly ScoreNormalizer _scoreNorm;
        private readonly VelocityNormalizer _velNorm;
        private readonly ImbalanceNormalizer _imbNorm;
        private readonly ExhaustionNormalizer _exhNorm;
        private readonly CompressionNormalizer _cmpNorm;

        private readonly MarketStateManager _marketStateManager;
        private readonly FlowMetricsRecorder _recorder;
        private readonly OpenInterestClient _openInterestClient;

        private readonly StrategyEngine _strategyEngine;
        private readonly StrategyAggregator _strategyAggregator;
        private readonly PositionManager _positionManager;
        private readonly TradeJournalRecorder _tradeJournal;

        private DateTime _lastSnapshot = DateTime.MinValue;
        private DateTime _lastOiFetch = DateTime.MinValue;
        private readonly ReplayTradeStreamClient? _replayClient;
        private long _lastReplayTimestamp = -1;
        private const long WarmupDurationMs = 12 * 60 * 60 * 1000;
        private double _lastOiValue = 0;
        private readonly bool _replayMode;
        private readonly ConcurrentQueue<FlowTrade> _tradeQueue = new();
        private readonly FlowEngineOptions _options;
        private DateTime _lastEngineCycle = DateTime.MinValue;
        private long _lastReplayEngineCycle = -1;
        private readonly FlowAggregationWindow _flowAggregationWindow;
        private readonly EngineState _state;
        public Worker(FlowTradeBuffer flowTradeBuffer, ITradeStreamClient tradeStreamClient, ILogger<Worker> logger, KinetixEngineProcessor engineProcessor, ScoreNormalizer scoreNorm, EngineBootstrapService bootstrap,
                    VelocityNormalizer velNorm, ImbalanceNormalizer imbNorm, ExhaustionNormalizer exhNorm, CompressionNormalizer cmpNorm, MarketStateManager snapshotManager, PositionManager positionManager,
                    EngineWarmupManager warmup, PriceTrendEngine priceEngine, ScoreTrendEngine scoreEngine, OpenInterestClient openInterestClient, FlowMetricsRecorder recorder, StrategyEngine strategyEngine,
                    StrategyAggregator strategyAggregator, TelegramService telegram, IOptions<FlowEngineOptions> options, TradeJournalRecorder tradeJournal, ExceptionAlertAggregator exceptionAggregator, ProbabilityTrendEngine probEngine, FlowAggregationWindow flowAggregationWindow)
        {
            _logger = logger;
            _bootstrap = bootstrap;
            _engineProcessor = engineProcessor;
            _warmup = warmup;
            _flowTradeBuffer = flowTradeBuffer;
            _tradeStreamClient = tradeStreamClient;
            _telegram = telegram;
            _scoreNorm = scoreNorm;
            _velNorm = velNorm;
            _imbNorm = imbNorm;
            _exhNorm = exhNorm;
            _cmpNorm = cmpNorm;
            _options = options.Value;

            _marketStateManager = snapshotManager;

            _priceEngine = priceEngine;
            _scoreEngine = scoreEngine;

            _strategyEngine = strategyEngine;
            _strategyAggregator = strategyAggregator;
            _positionManager = positionManager;
            _state = new EngineState();
            _openInterestClient = openInterestClient;
            _recorder = recorder;
            _replayMode = options.Value.ReplayMode;
            _flowAggregationWindow = flowAggregationWindow;
            _tradeStreamClient.OnTrade += trade =>
            {
                _flowTradeBuffer.AddTrade(trade);
                _flowAggregationWindow.AddTrade(trade);
                while (_tradeQueue.Count > 50000)
                    Thread.Sleep(1);

                _tradeQueue.Enqueue(trade);
            };

            _positionManager.Target1Reached += async trade =>
            {
                if (!trade.NotifyThroughTelegram)
                    return;

                if (!_replayMode)
                {
                    await _telegram.SendMessageAsync(
                            $"""
                        TARGET1 HIT | {trade.Direction} | Strategy: {trade.StrategyName}

                        Entry={trade.EntryPrice:F1}
                        Remaining={trade.RemainingSize:P0}

                        Score
                        Fast={_scoreEngine.Fast:F2}
                        Medium={_scoreEngine.Medium:F2}
                        Slow={_scoreEngine.Slow:F2}

                        Prob
                        Fast={_probEngine.Fast:F2}
                        Medium={_probEngine.Medium:F2}
                        Slow={_probEngine.Slow:F2}
                        """);
                }
            };
            _tradeJournal = tradeJournal;
            _positionManager.TradeClosed += (trade, exitPrice, exitTimestamp) =>
            {
                var duration = (exitTimestamp - trade.EntryTimeMs) / 1000;

                var entry = trade.EntryPrice;
                var target1 = trade.Target1;

                decimal pnlPoints;

                // SAME LOGIC AS TELEGRAM EXIT
                if (trade.Target1Hit)
                {
                    if (trade.Direction == SignalDirection.Long)
                    {
                        pnlPoints = (target1 - entry) * 0.7m + (exitPrice - entry) * 0.3m;
                    }
                    else
                    {
                        pnlPoints = (entry - target1) * 0.7m + (entry - exitPrice) * 0.3m;
                    }
                }
                else
                {
                    pnlPoints = trade.Direction == SignalDirection.Long ? exitPrice - entry : entry - exitPrice;
                }

                decimal risk = Math.Abs(entry - trade.StopLoss);
                decimal pnlR = risk == 0 ? 0 : pnlPoints / risk;
                decimal mfe = trade.Direction == SignalDirection.Long ? trade.MaxPrice - entry : entry - trade.MinPrice;
                decimal mae = trade.Direction == SignalDirection.Long ? entry - trade.MinPrice : trade.MaxPrice - entry;
                _tradeJournal.Record(new TradeJournalRecord
                {
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(exitTimestamp).UtcDateTime,
                    Strategy = trade.StrategyName,
                    Direction = trade.Direction,
                    EntryPrice = entry,
                    ExitPrice = exitPrice,
                    StopLoss = trade.StopLoss,
                    Target1 = target1,
                    DurationSeconds = duration,
                    PnlPoints = pnlPoints,
                    PnlR = pnlR,
                    MFE = mfe,
                    MAE = mae,
                    ScoreZ = trade.EntryScoreZ,
                    VelocityZ = trade.EntryVelocityZ,
                    ImbalanceZ = trade.EntryImbalanceZ,
                    CompressionZ = trade.EntryCompressionZ,
                    ATR = trade.EntryATR,
                    ER = trade.EntryER,
                    FlowState = trade.EntryFlowState
                });
            };
            _exceptionAggregator = exceptionAggregator;
            _probEngine = probEngine;

            if (_replayMode)
                _replayClient = tradeStreamClient as ReplayTradeStreamClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bootstrap.InitializeAsync(_replayMode);

            _ = Task.Run(() => _tradeStreamClient.StartAsync(stoppingToken), stoppingToken);
            if (!_replayMode)
            {
                var snapshot = _marketStateManager.Load();
                if (snapshot != null)
                {
                    var age = DateTime.UtcNow - snapshot.Timestamp;

                    //if (age.TotalMinutes < 60)
                    //{
                    _scoreNorm.Restore(snapshot.ScoreNormalizer);
                    _velNorm.Restore(snapshot.VelocityNormalizer);
                    _imbNorm.Restore(snapshot.ImbalanceNormalizer);
                    _exhNorm.Restore(snapshot.ExhaustionNormalizer);
                    _cmpNorm.Restore(snapshot.CompressionNormalizer);

                    _priceEngine.Restore(snapshot.PriceFastEma, snapshot.PriceSlowEma);
                    _scoreEngine.Restore(snapshot.ScoreFastEma, snapshot.ScoreSlowEma, snapshot.ScoreMediumEma);
                    _probEngine.Restore(snapshot.ProbFastEma, snapshot.ProbSlowEma, snapshot.ProbMediumEma);

                    _logger.LogInformation("Snapshot restored successfully.");
                    //}
                    //else
                    //{
                    //    await _telegram.SendMessageAsync("Market state is older than 60mins, fresh state started.");
                    //}
                }
            }
            _logger.LogInformation("Worker main loop STARTED at {Time} as {mode} mode.", DateTimeOffset.Now, _replayMode ? "Replay" : "Live");
            long replayCounter = 0;
            DateTime lastWarmupLog = DateTime.MinValue;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_tradeQueue.TryDequeue(out var lastTrade))
                    {
                        if (_replayMode && _replayClient?.Completed == true)
                        {
                            _logger.LogInformation("Replay finished. Engine stopping.");
                            break;
                        }

                        await Task.Delay(1);
                        continue;
                    }

                    if (_replayMode)
                    {
                        replayCounter++;

                        if (replayCounter % 50000 == 0)
                        {
                            _logger.LogInformation("REPLAY | Processed {Count:n0} timestamps | Queue {Queue}", replayCounter, _tradeQueue.Count);
                        }
                    }

                    double price = (double)lastTrade.Price;
                    if (_replayMode)
                    {
                        _lastOiValue = 0;
                    }
                    if (!_replayMode && (DateTime.UtcNow - _lastOiFetch).TotalSeconds > 120)
                    {
                        _lastOiValue = await _openInterestClient.GetOpenInterestAsync();
                        _lastOiFetch = DateTime.UtcNow;
                    }

                    KinetixEngineResult result;

                    try
                    {
                        result = _engineProcessor.Process(price, lastTrade.Quantity, _lastOiValue, lastTrade.Timestamp, _replayMode);
                    }
                    catch (Exception ex)
                    {
                        _exceptionAggregator.Capture(ex);
                        _logger.LogError(ex, "Engine processing failed");
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }
                    if (!_state.TradingEnabled)
                    {
                        if (_state.FirstTimestamp == 0)
                            _state.FirstTimestamp = lastTrade.Timestamp;

                        if (lastTrade.Timestamp - _state.FirstTimestamp >= WarmupDurationMs)
                        {
                            _state.TradingEnabled = true;
                            _logger.LogInformation("Engine warmup complete. Trading enabled.");
                        }
                    }

                    bool ready = _warmup.Update();
                    if (!ready)
                    {
                        if (_replayMode)
                        {
                            if (lastWarmupLog == DateTime.MinValue ||
                                (lastTrade.Timestamp - new DateTimeOffset(lastWarmupLog).ToUnixTimeMilliseconds()) > 30000)
                            {
                                _logger.LogInformation("ENGINE WARMUP | Waiting for indicators to stabilize...");
                                lastWarmupLog = DateTimeOffset.FromUnixTimeMilliseconds(lastTrade.Timestamp).UtcDateTime;
                            }
                        }
                        else
                        {
                            if ((DateTime.UtcNow - lastWarmupLog).TotalSeconds > 30)
                            {
                                _logger.LogInformation("ENGINE WARMUP | Waiting for indicators to stabilize...");
                                lastWarmupLog = DateTime.UtcNow;
                            }
                        }

                        if (!_replayMode)
                            await Task.Delay(10, stoppingToken);
                        else
                            await Task.Delay(1);

                        continue;
                    }

                    if (!_state.TradingEnabled)
                        continue;


                    // ────────────────────────────────────────────────
                    // Every trade — update stops & targets (both modes)
                    // ────────────────────────────────────────────────
                    if (_state.TradingEnabled)
                    {
                        _positionManager.Update((decimal)price, lastTrade.Timestamp);
                    }

                    // ────────────────────────────────────────────────
                    // Strategy decisions — only every N seconds
                    // ────────────────────────────────────────────────
                    bool shouldRunStrategy = false;
                    var cycleMs = _options.EngineCycleSeconds * 1000L;

                    if (_replayMode)
                    {
                        if (_lastReplayEngineCycle == -1)
                            _lastReplayEngineCycle = lastTrade.Timestamp - (lastTrade.Timestamp % cycleMs);

                        while (lastTrade.Timestamp >= _lastReplayEngineCycle + cycleMs)
                        {
                            long cycleTs = _lastReplayEngineCycle + cycleMs;

                            // No await here — run synchronously in replay tight loop
                            RunStrategyLogic(result, cycleTs);   // pass current price instead

                            _lastReplayEngineCycle = cycleTs;
                        }
                    }
                    else // live
                    {
                        if ((DateTime.UtcNow - _lastEngineCycle).TotalSeconds >= _options.EngineCycleSeconds)
                        {
                            shouldRunStrategy = true;
                            _lastEngineCycle = DateTime.UtcNow;
                        }
                    }

                    if (shouldRunStrategy || _replayMode /* already ran above */)
                    {
                        if (!_replayMode)
                        {
                            // live — run once per cycle
                            await RunStrategyLogic(result, lastTrade.Timestamp);
                        }
                        // replay already executed in while loop — no double execution
                    }

                    if (!_replayMode)
                        _recorder.Record(result);

                    if (!_replayMode)
                    {
                        _logger.LogInformation("FLOW | Price {Price:F2} RawScore {RawScore:F2} AdjScore {AdjScore:F2} " +
                                "Fast {Fast:F2} Medium {Medium:F2} Slow {Slow:F2} " + "ScoreZ {ScoreZ:F2} VelZ {VelZ:F2} ImbZ {ImbZ:F2} ExhZ {ExhZ:F2} CmpZ {CmpZ:F2} " +
                                "VWAP {VWAP:F2} ER5 {ER:F3} ER30 {ER30:F3} ATR {ATR:F2} OIΔ {OI:F2} " + "Trend {Trend} State {State} " +
                                "LongProb {LongProb:F3} ShortProb {ShortProb:F3} " + "LongStable {LongStable} ShortStable {ShortStable} " +
                                "LongPersist {LongPersist} ShortPersist {ShortPersist} " + "Impact {Impact:F3} ControlB {BullCtrl} ControlS {BearCtrl} " +
                                "WhaleStr B {WhaleStrB:F2} S {WhaleStrS:F2} " + "Pressure B {BuyP:F2} S {SellP:F2} Net {NetP:F2} | BFast {BFast:F4} BMedium {BMedium:F4} BSlow {BSlow:F4}",

                                result.Price, result.RawScore, result.AdjustedScore, result.ScoreFastEma, result.ScoreMediumEma, result.ScoreSlowEma, result.ScoreZ, result.VelocityZ,
                                result.ImbalanceZ, result.ExhaustionZ, result.CompressionZ, result.VWAP, result.ER, result.ER30, result.ATR, result.OIChange, result.ScoreTrend, result.FlowState.State,
                                result.LongProbability, result.ShortProbability, result.LongStable, result.ShortStable, result.LongPersistence, result.ShortPersistence, result.FlowImpactEfficiency,
                                result.BullishPriceControl, result.BearishPriceControl, result.BuyClusterStrength, result.SellClusterStrength, result.BuyPressure, result.SellPressure, result.NetPressure,
                                (result.ProbFastEma), (result.ProbMediumEma), (result.ProbSlowEma));
                    }

                    if (!_replayMode && (DateTime.UtcNow - _lastSnapshot).TotalSeconds > 60)
                    {
                        _marketStateManager.Save(new MarketStateSnapshot
                        {
                            Timestamp = DateTime.UtcNow,
                            LastPrice = price,

                            PriceFastEma = _priceEngine.Fast,
                            PriceSlowEma = _priceEngine.Slow,

                            ScoreFastEma = _scoreEngine.Fast,
                            ScoreSlowEma = _scoreEngine.Slow,
                            ScoreMediumEma = _scoreEngine.Medium,

                            ProbFastEma = _probEngine.Fast,
                            ProbSlowEma = _probEngine.Slow,
                            ProbMediumEma = _probEngine.Medium,

                            ScoreNormalizer = _scoreNorm.GetState(),
                            VelocityNormalizer = _velNorm.GetState(),
                            ImbalanceNormalizer = _imbNorm.GetState(),
                            ExhaustionNormalizer = _exhNorm.GetState(),
                            CompressionNormalizer = _cmpNorm.GetState()
                        });

                        _lastSnapshot = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error in main loop");
                }
                if (!_replayMode)
                    await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Worker main loop STOPPED");
        }

        private async Task RunStrategyLogic(KinetixEngineResult result, long timestamp)
        {
            decimal currentPrice = (decimal)result.Price;
            // ---------- EXIT FIRST (strategy exit only – stop loss already checked above) ----------
            foreach (var trade in _positionManager.GetAllPositions().ToList())
            {
                if (timestamp <= trade.EntryTimeMs) continue;

                var exitSignal = _strategyEngine.EvaluateExit(result, trade);
                if (exitSignal?.ExitSignal == true)
                {
                    _positionManager.CloseTrade(trade.StrategyName, currentPrice, timestamp);

                    if (trade.NotifyThroughTelegram && !_replayMode)
                    {
                        var exitPrice = currentPrice;
                        var entry = trade.EntryPrice;
                        var target1 = trade.Target1;
                        decimal pnlPoints;
                        if (trade.Target1Hit)
                        {
                            if (trade.Direction == SignalDirection.Long)
                            {
                                pnlPoints = (target1 - entry) * 0.7m + (exitPrice - entry) * 0.3m;
                            }
                            else
                            {
                                pnlPoints = (entry - target1) * 0.7m + (entry - exitPrice) * 0.3m;
                            }
                        }
                        else
                        {
                            pnlPoints = trade.Direction == SignalDirection.Long ? exitPrice - entry : entry - exitPrice;
                        }
                        var duration = (timestamp - trade.EntryTimeMs) / 1000;
                        if (!_replayMode)
                            await _telegram.SendMessageAsync(
                                    $"""
                                        EXIT | {trade.StrategyName} | {trade.Direction}

                                        Entry={entry:F1}  Exit={exitPrice:F1}

                                        PnL={pnlPoints:F1}
                                        Duration={(duration / 60):F1}Mins

                                        Score
                                        Fast={result.ScoreFastEma:F2}
                                        Medium={result.ScoreMediumEma:F2}
                                        Slow={result.ScoreSlowEma:F2}

                                        Prob
                                        Fast={result.ProbFastEma:F2}
                                        Medium={result.ProbMediumEma:F2}
                                        Slow={result.ProbSlowEma:F2}
                                    """);
                    }
                }
            }

            // ---------- ENTRY ----------
            var signals = _strategyEngine.Evaluate(result);
            foreach (var signal in signals)
            {
                if (signal.Direction == SignalDirection.None) continue;

                if (!_positionManager.HasPosition(signal.StrategyName))
                {
                    _positionManager.TryEnterTrade(signal, currentPrice, result.ATR15m, result, timestamp);

                    if (signal.NotifyThroughTelegram && !_replayMode)
                    {
                        var trade = _positionManager.GetPosition(signal.StrategyName);
                        if (!_replayMode)
                        {
                            await _telegram.SendMessageAsync(
                                        $"""
                                         ENTRY | {signal.Direction} | Strategy {signal.StrategyName}
                         
                                         Price={result.Price:F1}  SL={trade?.StopLoss:F1}
                                         TP1={trade?.Target1:F1}

                                         Score={result.ScoreZ:F2}
                                         ER={result.ER:F2}  ATR15={result.ATR15m:F1}
                                         State={result.FlowState.State}
                         
                                         Score
                                         Fast={result.ScoreFastEma:F2}
                                         Medium={result.ScoreMediumEma:F2}
                                         Slow={result.ScoreSlowEma:F2}

                                         Prob
                                         Fast={result.ProbFastEma:F2}
                                         Medium={result.ProbMediumEma:F2}
                                         Slow={result.ProbSlowEma:F2}
                                         """);
                            _logger.LogInformation("STRATEGY SIGNAL | Strategy {Strategy} Direction {Direction} Confidence {Confidence}",
                                signal.StrategyName, signal.Direction, signal.Confidence);
                        }
                    }
                }
            }
        }
    }
}
