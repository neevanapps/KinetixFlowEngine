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

        private DateTime _lastSnapshot = DateTime.MinValue;
        private DateTime _lastOiFetch = DateTime.MinValue;
        private double _lastOiValue = 0;

        private readonly FlowEngineOptions _options;
        private DateTime _lastEngineCycle = DateTime.MinValue;

        public Worker(FlowTradeBuffer flowTradeBuffer, TradeStreamClient tradeStreamClient, ILogger<Worker> logger, KinetixEngineProcessor engineProcessor, ScoreNormalizer scoreNorm, EngineBootstrapService bootstrap,
                    VelocityNormalizer velNorm, ImbalanceNormalizer imbNorm, ExhaustionNormalizer exhNorm, CompressionNormalizer cmpNorm, MarketStateManager snapshotManager, PositionManager positionManager,
                    EngineWarmupManager warmup, PriceTrendEngine priceEngine, ScoreTrendEngine scoreEngine, OpenInterestClient openInterestClient, FlowMetricsRecorder recorder, StrategyEngine strategyEngine,
                    StrategyAggregator strategyAggregator, TelegramService telegram, IOptions<FlowEngineOptions> options)
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

                await _telegram.SendMessageAsync($"TARGET1 HIT\n" + $"Strategy: {trade.StrategyName}\n" + $"Direction: {trade.Direction}\n" +
                    $"Entry: {trade.EntryPrice:F2}\n" + $"Remaining Size: {trade.RemainingSize:F2}");
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
                        _positionManager.CloseTrade();
                        if (trade.NotifyThroughTelegram)
                        {
                            await _telegram.SendMessageAsync($"EXIT SIGNAL\nStrategy: {trade.StrategyName}\nDirection: {trade.Direction}\nPrice: {result.Price:F2}");
                        }
                        continue;
                    }
                }

                // ---------- ENTRY SECOND ----------
                var signals = _strategyEngine.Evaluate(result);
                var finalSignal = _strategyAggregator.SelectSignal(signals);

                if (finalSignal != null && !_positionManager.HasPosition)
                {
                    _positionManager.TryEnterTrade(finalSignal, (decimal)result.Price, result.ATR15m);

                    if (finalSignal.NotifyThroughTelegram)
                    {
                        await _telegram.SendMessageAsync($"ENTRY SIGNAL\nStrategy: {finalSignal.StrategyName}\nDirection: {finalSignal.Direction}\nPrice: {result.Price:F2}");
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
