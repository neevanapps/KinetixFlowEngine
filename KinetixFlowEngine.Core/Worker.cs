using KinetixFlowEngine.Core.Bootstrap;
using KinetixFlowEngine.Core.Config;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Persistence;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core
{
    public class Worker : BackgroundService
    {
        private readonly TradeStreamClient _tradeStreamClient;
        private readonly FlowTradeBuffer _flowTradeBuffer;
        private readonly ILogger<Worker> _logger;
        private readonly KinetixEngineProcessor _engineProcessor;
        private readonly EngineWarmupManager _warmup;
        private readonly PriceTrendEngine _priceEngine;
        private readonly ScoreTrendEngine _scoreEngine;
        private readonly EngineBootstrapService _bootstrap;
        private readonly TelegramService _telegram;

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
        private double _lastOiValue = 0;

        private readonly FlowEngineOptions _options;
        private DateTime _lastEngineCycle = DateTime.MinValue;

        public Worker(FlowTradeBuffer flowTradeBuffer, TradeStreamClient tradeStreamClient, ILogger<Worker> logger, KinetixEngineProcessor engineProcessor, ScoreNormalizer scoreNorm, EngineBootstrapService bootstrap,
                    VelocityNormalizer velNorm, ImbalanceNormalizer imbNorm, ExhaustionNormalizer exhNorm, CompressionNormalizer cmpNorm, MarketStateManager snapshotManager, PositionManager positionManager,
                    EngineWarmupManager warmup, PriceTrendEngine priceEngine, ScoreTrendEngine scoreEngine, OpenInterestClient openInterestClient, FlowMetricsRecorder recorder, StrategyEngine strategyEngine,
                    StrategyAggregator strategyAggregator, TelegramService telegram, IOptions<FlowEngineOptions> options, TradeJournalRecorder tradeJournal)
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

            _openInterestClient = openInterestClient;
            _recorder = recorder;

            _tradeStreamClient.OnTrade += trade =>
            {
                _flowTradeBuffer.AddTrade(trade);
            };

            _positionManager.Target1Reached += async trade =>
            {
                if (!trade.NotifyThroughTelegram)
                    return;

                await _telegram.SendMessageAsync(
                    $"""
                        TARGET1 HIT | {trade.Direction}

                        Entry={trade.EntryPrice:F1}
                        Remaining={trade.RemainingSize:P0}

                        EMA
                        Fast={_scoreEngine.Fast:F2}
                        Medium={_scoreEngine.Medium:F2}
                        Slow={_scoreEngine.Slow:F2}
                    """);
            };
            _tradeJournal = tradeJournal;
            _positionManager.TradeClosed += (trade, exitPrice) =>
            {
                var duration = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - trade.EntryTimeMs) / 1000;
                decimal pnlPoints = trade.Direction == SignalDirection.Long ? exitPrice - trade.EntryPrice : trade.EntryPrice - exitPrice;
                decimal risk = Math.Abs(trade.EntryPrice - trade.StopLoss);
                decimal pnlR = risk == 0 ? 0 : pnlPoints / risk;
                decimal mfe = trade.Direction == SignalDirection.Long ? trade.MaxPrice - trade.EntryPrice : trade.EntryPrice - trade.MinPrice;
                decimal mae = trade.Direction == SignalDirection.Long ? trade.EntryPrice - trade.MinPrice : trade.MaxPrice - trade.EntryPrice;
                _tradeJournal.Record(new TradeJournalRecord
                {
                    Timestamp = DateTime.UtcNow,
                    Strategy = trade.StrategyName,
                    Direction = trade.Direction,
                    EntryPrice = trade.EntryPrice,
                    ExitPrice = exitPrice,
                    StopLoss = trade.StopLoss,
                    Target1 = trade.Target1,
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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bootstrap.InitializeAsync();

            await _tradeStreamClient.StartAsync(stoppingToken);

            var snapshot = _marketStateManager.Load();

            if (snapshot != null)
            {
                var age = DateTime.UtcNow - snapshot.Timestamp;

                if (age.TotalMinutes < 60)
                {
                    _scoreNorm.Restore(snapshot.ScoreNormalizer);
                    _velNorm.Restore(snapshot.VelocityNormalizer);
                    _imbNorm.Restore(snapshot.ImbalanceNormalizer);
                    _exhNorm.Restore(snapshot.ExhaustionNormalizer);
                    _cmpNorm.Restore(snapshot.CompressionNormalizer);

                    _priceEngine.Restore(snapshot.PriceFastEma, snapshot.PriceSlowEma);
                    _scoreEngine.Restore(snapshot.ScoreFastEma, snapshot.ScoreSlowEma, snapshot.ScoreMediumEma);

                    _logger.LogInformation("Snapshot restored successfully.");
                }
                else
                {
                    await _telegram.SendMessageAsync("Market state is older than 60mins, fresh state started.");
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_flowTradeBuffer.TryGetLast(out var lastTrade))
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }
                double price = (double)lastTrade.Price;

                if ((DateTime.UtcNow - _lastOiFetch).TotalSeconds > 120)
                {
                    _lastOiValue = await _openInterestClient.GetOpenInterestAsync();
                    _lastOiFetch = DateTime.UtcNow;
                }
                if ((DateTime.UtcNow - _lastEngineCycle).TotalSeconds < _options.EngineCycleSeconds)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }
                _lastEngineCycle = DateTime.UtcNow;

                var result = _engineProcessor.Process(price, lastTrade.Quantity, _lastOiValue);
                bool ready = _warmup.Update();
                if (!ready)
                {
                    if ((DateTime.UtcNow.Second % 30) == 0)
                    {
                        _logger.LogInformation("ENGINE WARMUP | Waiting for indicators to stabilize...");
                    }
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                // ---------- EXIT FIRST ----------
                if (_positionManager.HasPosition)
                {
                    var trade = _positionManager.ActiveTrade;
                    var exitSignal = _strategyEngine.EvaluateExit(result, trade);
                    if (exitSignal != null)
                    {
                        _positionManager.CloseTrade((decimal)price);
                        if (trade.NotifyThroughTelegram)
                        {
                            var exitPrice = (decimal)result.Price;
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
                            var duration = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - trade.EntryTimeMs) / 1000;
                            await _telegram.SendMessageAsync(
                            $"""
                                EXIT | {trade.Direction}

                                Entry={entry:F1}  Exit={exitPrice:F1}

                                PnL={pnlPoints:F1}
                                Duration={(duration / 60): F1}Mins

                                EMA
                                Fast={result.ScoreFastEma:F2}
                                Medium={result.ScoreMediumEma:F2}
                                Slow={result.ScoreSlowEma:F2}
                            """);
                        }
                        continue;
                    }
                }

                // ---------- ENTRY SECOND ----------
                var signals = _strategyEngine.Evaluate(result);
                var finalSignal = _strategyAggregator.SelectSignal(signals);

                if (finalSignal != null && !_positionManager.HasPosition)
                {
                    _positionManager.TryEnterTrade(finalSignal, (decimal)result.Price, result.ATR15m, result);

                    if (finalSignal.NotifyThroughTelegram)
                    {
                        var trade = _positionManager.ActiveTrade;

                        await _telegram.SendMessageAsync(
                        $"""
                                ENTRY | {finalSignal.Direction}

                                Price={result.Price:F1}  SL={trade?.StopLoss:F1}

                                Score={result.ScoreZ:F2}  Conf={(result.LongProbability * 100):F0}%
                                ER={result.ER:F2}  ATR15={result.ATR15m:F1}
                                State={result.FlowState.State}

                                EMA
                                Fast={result.ScoreFastEma:F2}
                                Medium={result.ScoreMediumEma:F2}
                                Slow={result.ScoreSlowEma:F2}
                         """);
                    }
                    _logger.LogInformation("STRATEGY SIGNAL | Strategy {Strategy} Direction {Direction} Confidence {Confidence}",
                        finalSignal.StrategyName, finalSignal.Direction, finalSignal.Confidence);
                }
                _positionManager.Update((decimal)result.Price);

                _recorder.Record(result);
                _logger.LogInformation("FLOW | " + "Price {Price:F2} " + "RawScore {RawScore:F2} AdjScore {AdjScore:F2} " + "Fast {Fast:F2} Medium {Medium:F2} Slow {Slow:F2} "
                    + "ScoreZ {ScoreZ:F2} VelZ {VelZ:F2} ImbZ {ImbZ:F2} ExhZ {ExhZ:F2} CmpZ {CmpZ:F2} " + "VWAP {VWAP:F2} ER {ER:F3} ATR {ATR:F2} OIΔ {OI:F2} " + "Trend {Trend} "
                    + "State {State} " + "LongProb {LongProb:F3} ShortProb {ShortProb:F3} " + "LongStable {LongStable} ShortStable {ShortStable} " + "LongPersist {LongPersist} ShortPersist {ShortPersist}",
                    result.Price, result.RawScore, result.AdjustedScore, result.ScoreFastEma, result.ScoreMediumEma, result.ScoreSlowEma, result.ScoreZ, result.VelocityZ, result.ImbalanceZ, result.ExhaustionZ,
                    result.CompressionZ, result.VWAP, result.ER, result.ATR, result.OIChange, result.ScoreTrend, result.FlowState.State, result.LongProbability, result.ShortProbability,
                    result.LongStable, result.ShortStable, result.LongPersistence, result.ShortPersistence);

                if ((DateTime.UtcNow - _lastSnapshot).TotalSeconds > 60)
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

                        ScoreNormalizer = _scoreNorm.GetState(),
                        VelocityNormalizer = _velNorm.GetState(),
                        ImbalanceNormalizer = _imbNorm.GetState(),
                        ExhaustionNormalizer = _exhNorm.GetState(),
                        CompressionNormalizer = _cmpNorm.GetState()
                    });

                    _lastSnapshot = DateTime.UtcNow;
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
