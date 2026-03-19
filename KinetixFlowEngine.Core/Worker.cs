using KinetixFlowEngine.Core.Bootstrap;
using KinetixFlowEngine.Core.Config;
using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Persistence;
using KinetixFlowEngine.Core.Prop;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Options;
using System.Globalization;

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
        private readonly ProbabilityTrendEngine _probEngine;
        private readonly EngineBootstrapService _bootstrap;
        private readonly TelegramService _telegram;
        private readonly ExceptionAlertAggregator _exceptionAggregator;
        private readonly FlowAggregationWindow _flowAggregationWindow;

        private readonly ScoreNormalizer _scoreNorm;
        private readonly VelocityNormalizer _velNorm;
        private readonly ImbalanceNormalizer _imbNorm;
        private readonly ExhaustionNormalizer _exhNorm;
        private readonly CompressionNormalizer _cmpNorm;
        private readonly FairPriceEngine _fairPriceEngine;
        private readonly MarketStateManager _marketStateManager;
        private readonly FlowMetricsRecorder _recorder;
        private readonly OpenInterestClient _openInterestClient;

        private readonly StrategyEngine _strategyEngine;
        private readonly StrategyAggregator _strategyAggregator;
        private readonly PositionManager _positionManager;
        private readonly TradeJournalRecorder _tradeJournal;
        private readonly FlowMomentumRun _momentumRun;
        private readonly TradeMemoryManager _tradeMemory;
        private readonly VolumeEngine _volumeEngine;
        private DateTime _lastSnapshot = DateTime.MinValue;
        private DateTime _lastOiFetch = DateTime.MinValue;
        private double _lastOiValue = 0;
        private readonly PositionPersistence _positionPersistence;
        private readonly PropAccountStatePersistence _accountStatePersistence;
        private readonly PropAlertService _alerts;
        private readonly PropOrchestrator _propOrchestrator;
        private readonly List<AccountRuntime> _accounts;
        private readonly StrategyConfigLoader _strategyConfigLoader;
        private readonly FlowEngineOptions _options;
        private DateTime _lastEngineCycle = DateTime.MinValue;

        public Worker(FlowTradeBuffer flowTradeBuffer, TradeStreamClient tradeStreamClient, ILogger<Worker> logger, KinetixEngineProcessor engineProcessor, ScoreNormalizer scoreNorm, EngineBootstrapService bootstrap,
                    VelocityNormalizer velNorm, ImbalanceNormalizer imbNorm, ExhaustionNormalizer exhNorm, CompressionNormalizer cmpNorm, MarketStateManager snapshotManager, PositionManager positionManager,
                    EngineWarmupManager warmup, PriceTrendEngine priceEngine, ScoreTrendEngine scoreEngine, OpenInterestClient openInterestClient, FlowMetricsRecorder recorder, StrategyEngine strategyEngine,
                    StrategyAggregator strategyAggregator, TelegramService telegram, IOptions<FlowEngineOptions> options, TradeJournalRecorder tradeJournal, ExceptionAlertAggregator exceptionAggregator,
                    ProbabilityTrendEngine probEngine, FlowAggregationWindow flowAggregationWindow, FlowMomentumRun momentumRun, TradeMemoryManager tradeMemory, VolumeEngine volumeEngine, FairPriceEngine fairPriceEngine,
                    PropOrchestrator propOrchestrator, List<AccountRuntime> accounts, PropAlertService alerts, PropAccountStatePersistence accountStatePersistence, PositionPersistence positionPersistence, StrategyConfigLoader strategyConfigLoader)
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
            _tradeMemory = tradeMemory;
            _propOrchestrator = propOrchestrator;
            _marketStateManager = snapshotManager;
            _priceEngine = priceEngine;
            _scoreEngine = scoreEngine;
            _strategyEngine = strategyEngine;
            _strategyAggregator = strategyAggregator;
            _positionManager = positionManager;
            _openInterestClient = openInterestClient;
            _recorder = recorder;
            _flowAggregationWindow = flowAggregationWindow;
            _tradeJournal = tradeJournal;
            _exceptionAggregator = exceptionAggregator;
            _probEngine = probEngine;
            _momentumRun = momentumRun;
            _volumeEngine = volumeEngine;
            _fairPriceEngine = fairPriceEngine;
            _accounts = accounts;
            _alerts = alerts;
            _accountStatePersistence = accountStatePersistence;
            _positionPersistence = positionPersistence;
            _strategyConfigLoader = strategyConfigLoader;

            _tradeStreamClient.OnTrade += trade =>
            {
                _flowTradeBuffer.AddTrade(trade);
                _flowAggregationWindow.AddTrade(trade);
            };

            _positionManager.Target1Reached += async trade =>
            {
                if (!trade.NotifyThroughTelegram)
                    return;

                await _alerts.SendTarget1Async(trade);
            };
            _positionManager.TradeClosed += (trade, exitPrice) =>
            {
                var duration = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - trade.EntryTimeMs) / 1000;
                var entry = trade.EntryPrice;
                var target1 = trade.Target1;

                // -------------------------------
                // ✅ CORRECT PnL WITH SIZE
                // -------------------------------

                var config = _strategyConfigLoader.Get(trade.StrategyName);

                decimal t1Percent = config?.Target1SizePercent > 0
                    ? config.Target1SizePercent / 100m
                    : 0.5m;

                // -------------------------------
                // PnL WITH BREAKDOWN
                // -------------------------------
                decimal gross, fee, pnl;

                if (trade.Target1Hit)
                {
                    // partial → approximate breakdown (still correct net)
                    var result = PropPnLCalculator.CalculatePartial(
                        trade.EntryPrice,
                        trade.Target1,
                        exitPrice,
                        trade.InitialSize,
                        t1Percent,
                        trade.Direction,
                        _options.FeeRate);

                    // fallback (we don’t split gross/fee per leg precisely)
                    var breakdown = PropPnLCalculator.CalculateWithBreakdown(
                        trade.EntryPrice,
                        exitPrice,
                        trade.InitialSize,
                        trade.Direction,
                        _options.FeeRate);

                    gross = breakdown.gross;
                    fee = breakdown.fee;
                    pnl = result;
                }
                else
                {
                    var breakdown = PropPnLCalculator.CalculateWithBreakdown(
                        trade.EntryPrice,
                        exitPrice,
                        trade.InitialSize,
                        trade.Direction,
                        _options.FeeRate);

                    gross = breakdown.gross;
                    fee = breakdown.fee;
                    pnl = breakdown.net;
                }
                // -------------------------------
                // JOURNAL
                // -------------------------------
                decimal riskUsd = Math.Abs(trade.EntryPrice - trade.StopLoss) * trade.InitialSize;
                decimal pnlR = riskUsd == 0 ? 0 : pnl / riskUsd;
                decimal mfe = trade.Direction == SignalDirection.Long
                    ? (trade.MaxPrice - entry) * trade.InitialSize
                    : (entry - trade.MinPrice) * trade.InitialSize;

                decimal mae = trade.Direction == SignalDirection.Long
                    ? (entry - trade.MinPrice) * trade.InitialSize
                    : (trade.MaxPrice - entry) * trade.InitialSize;

                _tradeJournal.Record(new TradeJournalRecord
                {
                    Timestamp = DateTime.UtcNow,
                    Strategy = trade.StrategyName,
                    Direction = trade.Direction,
                    EntryPrice = trade.EntryPrice,
                    ExitPrice = exitPrice,
                    StopLoss = trade.StopLoss,
                    Target1 = trade.Target1,
                    Size = trade.InitialSize,
                    DurationSeconds = duration,
                    PnlUsd = pnl,            
                    GrossPnlUsd = gross,       
                    FeeUsd = fee,
                    PnlR = pnlR,
                    MFE = trade.Direction == SignalDirection.Long
                                    ? (trade.MaxPrice - trade.EntryPrice) * trade.InitialSize
                                    : (trade.EntryPrice - trade.MinPrice) * trade.InitialSize,
                    MAE = trade.Direction == SignalDirection.Long
                                 ? (trade.EntryPrice - trade.MinPrice) * trade.InitialSize
                                 : (trade.MaxPrice - trade.EntryPrice) * trade.InitialSize,
                    ScoreZ = trade.EntryScoreZ,
                    VelocityZ = trade.EntryVelocityZ,
                    ImbalanceZ = trade.EntryImbalanceZ,
                    CompressionZ = trade.EntryCompressionZ,
                    ATR = (decimal)trade.EntryATR,
                    ER = (decimal)trade.EntryER,
                    FlowState = trade.EntryFlowState,
                });

                // -------------------------------
                // MEMORY
                // -------------------------------
                _tradeMemory.Record(trade.AccountId, new TradeMemory
                {
                    StrategyName = trade.StrategyName,
                    Direction = trade.Direction,
                    EntryPrice = trade.EntryPrice,
                    ExitPrice = exitPrice,
                    ExitReason = trade.ExitReason,
                    ExitTime = DateTime.UtcNow
                });

                // -------------------------------
                // ✅ FIXED EQUITY UPDATE
                // -------------------------------
                var account = _accounts.FirstOrDefault(x => x.Config.AccountId == trade.AccountId);

                if (account != null)
                {
                    // 🔴 CRITICAL FIX
                    account.State.CurrentEquity += pnl;
                    _accountStatePersistence.Update(account.Config.AccountId, account.State);
                }
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bootstrap.InitializeAsync();

            await _tradeStreamClient.StartAsync(stoppingToken);


            var persisted = _positionPersistence.Load();
            if (persisted.Any())
            {
                _positionManager.Restore(persisted);
                _logger.LogInformation("Restored {count} open positions", persisted.Count);
            }

            // ---- RESTORE PROP ACCOUNT STATE ----
            foreach (var acc in _accounts)
            {
                var state = _accountStatePersistence.Load(acc.Config.AccountId);

                if (state != null)
                {
                    acc.State = state;
                    _logger.LogInformation("Restored account state for {account}", acc.Config.AccountId);
                }
                else
                {
                    _logger.LogWarning("No persisted state found for {account}, using defaults", acc.Config.AccountId);
                }
            }

            var snapshot = _marketStateManager.Load();
            if (snapshot != null)
            {
                _scoreNorm.Restore(snapshot.ScoreNormalizer);
                _velNorm.Restore(snapshot.VelocityNormalizer);
                _imbNorm.Restore(snapshot.ImbalanceNormalizer);
                _exhNorm.Restore(snapshot.ExhaustionNormalizer);
                _cmpNorm.Restore(snapshot.CompressionNormalizer);

                if (snapshot.MomentumRun == 0)
                {
                    _momentumRun.Bootstrap(0); // neutral start
                }
                else
                    _momentumRun.Restore(snapshot.MomentumRun);

                _priceEngine.Restore(snapshot.PriceFastEma, snapshot.PriceSlowEma);
                _scoreEngine.Restore(snapshot.ScoreFastEma, snapshot.ScoreSlowEma, snapshot.ScoreMediumEma);
                _probEngine.Restore(snapshot.ProbFastEma, snapshot.ProbSlowEma, snapshot.ProbMediumEma);

                _logger.LogInformation("Snapshot restored successfully.");
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

                KinetixEngineResult result;

                try
                {
                    result = _engineProcessor.Process(price, lastTrade.Quantity, lastTrade.Timestamp, _lastOiValue);
                }
                catch (Exception ex)
                {
                    _exceptionAggregator.Capture(ex);
                    _logger.LogError(ex, "Engine processing failed");
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

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
                foreach (var trade in _positionManager.GetAllPositions().ToList())
                {
                    var exitSignal = _strategyEngine.EvaluateExit(result, trade);

                    if (exitSignal?.ExitSignal == true)
                    {
                        _positionManager.CloseTrade(trade.StrategyName, trade.AccountId, (decimal)price, "SignalFlip");
                        if (trade.NotifyThroughTelegram)
                        {
                            var exitPrice = (decimal)price;
                            var entry = trade.EntryPrice;
                            var target1 = trade.Target1;
                            decimal pnl = PropPnLCalculator.Calculate(trade.EntryPrice, exitPrice, trade.InitialSize, trade.Direction, _options.FeeRate);
                            var duration = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - trade.EntryTimeMs) / 1000;
                            var acc = _accounts.FirstOrDefault(x => x.Config.AccountId == trade.AccountId);

                            if (acc != null && trade.NotifyThroughTelegram)
                            {
                                await _alerts.SendExitAsync(
                                    trade,
                                    exitPrice,
                                    pnl,
                                    0m,
                                    acc.State.CurrentEquity,
                                    acc.State.DailyDrawdownPct,
                                    acc.State.OverallDrawdownPct);
                            }
                        }
                        continue;
                    }
                }

                // ---------- ENTRY SECOND ----------
                var signals = _strategyEngine.Evaluate(result);
                foreach (var acc in _accounts)
                {
                    if (!acc.Config.Enabled)
                        continue;

                    foreach (var signal in signals)
                    {
                        if (signal.Direction == SignalDirection.None)
                            continue;

                        bool isFairPrice = signal.Direction == SignalDirection.Long ?
                            _fairPriceEngine.IsFairLongEntry((decimal)price, result.VWAP, result.ATR) :
                            _fairPriceEngine.IsFairShortEntry((decimal)price, result.VWAP, result.ATR);

                        bool isVolumeExpansion = _volumeEngine.IsVolumeExpansion();

                        if (!IsReentryAllowed(signal, result, (decimal)price, isFairPrice, isVolumeExpansion, acc.Config.AccountId))
                            continue;

                        _propOrchestrator.ProcessSignal(signal, (decimal)result.Price, (decimal)result.ATR15m, result);

                        if (signal.NotifyThroughTelegram)
                        {
                            var newTrades = _positionManager.GetAllPositions()
                                .Where(t => t.StrategyName == signal.StrategyName &&
                                            t.AccountId == acc.Config.AccountId &&
                                            !t.EntryAlertSent &&
                                            !t.Closed);

                            foreach (var trade in newTrades)
                            {
                                await _alerts.SendEntryAsync(
                                    trade,
                                    acc.State.CurrentEquity,
                                    acc.State.DailyDrawdownPct,
                                    acc.State.OverallDrawdownPct);

                                trade.EntryAlertSent = true;
                            }
                            //_logger.LogInformation("STRATEGY SIGNAL | Account {Account} Strategy {Strategy} Direction {Direction}", acc.Config.AccountId, signal.StrategyName, signal.Direction);
                        }
                    }
                }
                _positionManager.Update((decimal)price);
                await _propOrchestrator.UpdateEquity((decimal)price);

                _recorder.Record(result);
                _logger.LogInformation("FLOW | P {Price:F2} Raw {RawScore:F2} Adj {AdjScore:F2} | " + "FS {Fast:F2} MS {Medium:F2} SS {Slow:F2}" +
                            " | VWAP {VWAP:F2} ER5 {ER:F2} ER30 {ER30:F2} ATR {ATR:F2} " + "| B {BuyP:F2} S {SellP:F2} Net {NetP:F2} | FP {BFast:F4} MP {BMedium:F4} SP {BSlow:F4} | v15 {v15:F2} v1 {v1:F2} F {Factor:F3}",
                            result.Price, result.RawScore, result.AdjustedScore, result.ScoreFastEma, result.ScoreMediumEma, result.ScoreSlowEma, result.VWAP, result.ER, result.ER30, result.ATR,
                            result.BuyPressure, result.SellPressure, result.NetPressure,
                            (result.ProbFastEma), (result.ProbMediumEma), (result.ProbSlowEma), result.Volume15, result.Volume1, result.TrendFactor);

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

                        ProbFastEma = _probEngine.Fast,
                        ProbSlowEma = _probEngine.Slow,
                        ProbMediumEma = _probEngine.Medium,
                        MomentumRun = _momentumRun.Run,

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

        private bool IsReentryAllowed(StrategySignal signal, KinetixEngineResult r, decimal price, bool isFairPrice, bool isVolumeExpansion, string accountId)
        {
            //Naveen need to make it account wise
            // No previous trade → allow
            var last = _tradeMemory.Get(signal.StrategyName, accountId);
            if (last == null)
                return true;

            // Only restrict SAME direction after SL / TSL
            if (last.Direction != signal.Direction)
                return true;

            if (last.ExitReason != "SL" && last.ExitReason != "TSL")
                return true;

            // -------------------------------
            // Path A: Pullback entry
            // -------------------------------
            if (isFairPrice && isVolumeExpansion)
                return true;

            // -------------------------------
            // Path B: Breakout entry
            // -------------------------------
            if (signal.Direction == SignalDirection.Long && (decimal)r.ProbMediumEma >= 0.60m && isVolumeExpansion)
                return true;

            if (signal.Direction == SignalDirection.Short && (decimal)r.ProbMediumEma <= 0.40m && isVolumeExpansion)
                return true;

            return false;
        }
    }
}
