using KinetixFlowEngine.Core.Bootstrap;
using KinetixFlowEngine.Core.Config;
using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Depth;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Execution;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Persistence;
using KinetixFlowEngine.Core.Prop;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Options;
using Serilog;
using System.Globalization;
using System.Text;

namespace KinetixFlowEngine.Core
{
    public class Worker : BackgroundService
    {
        private readonly BinanceDepthStreamClient _depthStreamClient;
        private readonly TradeStreamClient _tradeStreamClient;
        private readonly BybitDepthStreamClient _depthClient;
        private readonly FlowTradeBuffer _flowTradeBuffer;
        private readonly ILogger<Worker> _logger;
        private readonly KinetixEngineProcessor _engineProcessor;
        private readonly EngineWarmupManager _warmup;
        private readonly PriceTrendEngine _priceEngine;
        private readonly ScoreTrendEngine _scoreEngine;
        private readonly ProbabilityTrendEngine _probEngine;
        private readonly EngineBootstrapService _bootstrap;
        private readonly INotificationService _notificationService;
        private readonly ExceptionAlertAggregator _exceptionAggregator;
        private readonly FlowAggregationWindow _flowAggregationWindow;
        private readonly DepthFeatureEngine _depthFeatureEngine;
        private readonly DepthMinuteAggregator _depthMinuteAggregator;
        private readonly DepthWallTracker _depthWallTracker;
        private readonly DepthFeatureManager _depthFeatureManager;

        private readonly ScoreNormalizer _scoreNorm;
        private readonly VelocityNormalizer _velNorm;
        private readonly ImbalanceNormalizer _imbNorm;
        private readonly ExhaustionNormalizer _exhNorm;
        private readonly CompressionNormalizer _cmpNorm;
        private readonly AdjustedScoreNormalizer _adjustmentNorm;

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

        private DateTime _lastDepthLogUtc;
        private DateTime _lastGptReviewUtc = DateTime.MinValue;
        private DateTime _lastMarketStateSaveUtc = DateTime.MinValue;
        private DateTime _engineStartTimeUtc = DateTime.UtcNow;
        private DateTime _lastSnapshot = DateTime.MinValue;
        private DateTime _lastOiFetch = DateTime.MinValue;
        private DateTime _lastDailyResetUtc = DateTime.MinValue;
        private DateTime _lastSync = DateTime.MinValue;
        private DateTime _lastPositionLog = DateTime.MinValue;
        private int _drawdownCheckCounter = 0;
        private const int DRAW_DOWN_PERSIST_EVERY_N_CYCLES = 5;   // ~every 5–10 seconds
        private double _lastOiValue = 0;

        private readonly PositionPersistence _positionPersistence;
        private readonly PropAccountStatePersistence _accountStatePersistence;
        private readonly PropAlertService _alerts;
        private readonly PropAccountRuntimeManager _accounts;
        private readonly StrategyConfigLoader _strategyConfigLoader;
        private readonly FlowEngineOptions _options;
        private DateTime _lastEngineCycle = DateTime.MinValue;
        private readonly IExecutionRouter _router;
        private readonly IEquityEngine _equityEngine;
        private readonly ExecutionSyncService _executionSync;
        private readonly ITradeExecutor _executor;
        private decimal _currentMarketPrice = 0m;
        private readonly BybitClientFactory _factory;
        private readonly FundingRateClient _fundingRateClient;
        private readonly FundingRateEngine _fundingRateEngine;
        private readonly AtrNormalizer _atrNormalizer;
        private readonly EmaStability _emaStability;
        private string _lastFlowSnapshot = string.Empty;
        private readonly SignalStrengthEngine _strengthEngine;

        private readonly GptSnapshotBuilder _gptSnapshotBuilder;
        private readonly GptSnapshotStore _gptSnapshotStore;
        private readonly IGptSessionManager _gptSessionManager;
        private readonly IGptPromptBuilder _gptPromptBuilder;
        private readonly IGptReviewService _gptReviewService;
        private readonly GptMarketStateManager _gptMarketStateManager;
        private readonly GptMultiTimeframeAggregator _mtfAggregator;
        private readonly GptMarketSnapshotV2Builder _snapshotV2Builder;
        private readonly IGptReviewQueue _gptReviewQueue;
        public Worker(FlowTradeBuffer flowTradeBuffer, TradeStreamClient tradeStreamClient, ILogger<Worker> logger, KinetixEngineProcessor engineProcessor, ScoreNormalizer scoreNorm, EngineBootstrapService bootstrap,
                    VelocityNormalizer velNorm, ImbalanceNormalizer imbNorm, ExhaustionNormalizer exhNorm, CompressionNormalizer cmpNorm, MarketStateManager snapshotManager, PositionManager positionManager,
                    EngineWarmupManager warmup, PriceTrendEngine priceEngine, ScoreTrendEngine scoreEngine, OpenInterestClient openInterestClient, FlowMetricsRecorder recorder, StrategyEngine strategyEngine,
                    StrategyAggregator strategyAggregator, INotificationService notificationService, IOptions<FlowEngineOptions> options, TradeJournalRecorder tradeJournal, ExceptionAlertAggregator exceptionAggregator,
                    ProbabilityTrendEngine probEngine, FlowAggregationWindow flowAggregationWindow, FlowMomentumRun momentumRun, TradeMemoryManager tradeMemory, VolumeEngine volumeEngine, FairPriceEngine fairPriceEngine,
                    PropAccountRuntimeManager accounts, PropAlertService alerts, PropAccountStatePersistence accountStatePersistence, PositionPersistence positionPersistence, BybitClientFactory factory,
                    StrategyConfigLoader strategyConfigLoader, IExecutionRouter executionRouter, IEquityEngine equityEngine, ExecutionSyncService executionSync, ITradeExecutor executor, BybitDepthStreamClient depthClient,
                    AdjustedScoreNormalizer adjustmentNorm, FundingRateClient fundingRateClient, FundingRateEngine fundingRateEngine, AtrNormalizer atrNormalizer, EmaStability ema, SignalStrengthEngine strengthEngine,
                    GptSnapshotBuilder gptSnapshotBuilder, GptSnapshotStore gptSnapshotStore, IGptSessionManager gptSessionManager, IGptPromptBuilder gptPromptBuilder, IGptReviewService gptReviewService,
                    GptMarketStateManager gptMarketStateManager, GptMultiTimeframeAggregator mtfAggregator, GptMarketSnapshotV2Builder snapshotV2Builder, IGptReviewQueue gptReviewQueue, BinanceDepthStreamClient depthStreamClient,
                    DepthFeatureEngine depthFeatureEngine, DepthMinuteAggregator depthMinuteAggregator, DepthWallTracker depthWallTracker, DepthFeatureManager depthFeatureManager)
        {
            _logger = logger;
            _bootstrap = bootstrap;
            _engineProcessor = engineProcessor;
            _warmup = warmup;
            _flowTradeBuffer = flowTradeBuffer;
            _tradeStreamClient = tradeStreamClient;
            _notificationService = notificationService;
            _scoreNorm = scoreNorm;
            _velNorm = velNorm;
            _imbNorm = imbNorm;
            _exhNorm = exhNorm;
            _cmpNorm = cmpNorm;
            _options = options.Value;
            _tradeMemory = tradeMemory;
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
            _router = executionRouter;
            _equityEngine = equityEngine;
            _executionSync = executionSync;
            _executor = executor;
            _depthClient = depthClient;
            _factory = factory;
            _adjustmentNorm = adjustmentNorm;
            _fundingRateClient = fundingRateClient;
            _fundingRateEngine = fundingRateEngine;
            _atrNormalizer = atrNormalizer;
            _emaStability = ema;
            _strengthEngine = strengthEngine;
            _gptSnapshotBuilder = gptSnapshotBuilder;
            _gptSnapshotStore = gptSnapshotStore;
            _gptSessionManager = gptSessionManager;
            _gptPromptBuilder = gptPromptBuilder;
            _gptReviewService = gptReviewService;
            _gptMarketStateManager = gptMarketStateManager;
            _mtfAggregator = mtfAggregator;
            _snapshotV2Builder = snapshotV2Builder;
            _gptReviewQueue = gptReviewQueue;
            _depthStreamClient = depthStreamClient;
            _depthFeatureEngine = depthFeatureEngine;
            _depthMinuteAggregator = depthMinuteAggregator;
            _depthWallTracker = depthWallTracker;
            _depthFeatureManager = depthFeatureManager;

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
            _positionManager.TradeClosed += async (trade, exitPrice) =>
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

                //Apply PnL to Equity
                var account = _accounts.Accounts.FirstOrDefault(x => x.Config.AccountId == trade.AccountId);
                if (account == null) return;

                var client = _factory.GetClient(
                    trade.AccountId,
                    account.Config.ApiKey,
                    account.Config.ApiSecret);

                // Fetch real balance after close
                decimal realBalance = await client.GetUsdtWalletBalanceAsync();

                if (realBalance > 0)
                {
                    decimal oldEquity = account.State.CurrentEquity;
                    account.State.CurrentEquity = realBalance;

                    // Update HWMs if higher
                    if (realBalance > account.State.HighWaterMarkDaily)
                        account.State.HighWaterMarkDaily = realBalance;

                    if (realBalance > account.State.HighWaterMarkOverall)
                        account.State.HighWaterMarkOverall = realBalance;

                    // Recalculate DD %
                    account.State.DailyDrawdownPct = account.State.HighWaterMarkDaily > 0
                        ? Math.Max(0, (account.State.HighWaterMarkDaily - realBalance) / account.State.HighWaterMarkDaily * 100m)
                        : 0m;

                    account.State.OverallDrawdownPct = account.State.HighWaterMarkOverall > 0
                        ? Math.Max(0, (account.State.HighWaterMarkOverall - realBalance) / account.State.HighWaterMarkOverall * 100m)
                        : 0m;

                    // Optional: log difference
                    if (Math.Abs(realBalance - oldEquity) > 0.01m)
                    {
                        _logger.LogInformation("Trade close balance sync | {Account} | Old={Old} → Real={Real} | Delta={Delta}",
                            trade.AccountId, oldEquity, realBalance, realBalance - oldEquity);
                    }

                    _accountStatePersistence.Update(trade.AccountId, account.State);
                }
            };

            _positionManager.PartialCloseRequested += async trade =>
            {
                if (trade.AccountId == "SIM")
                    return;

                var acc = _accounts.Accounts.First(x => x.Config.AccountId == trade.AccountId);

                var reduceQty = trade.InitialSize - trade.RemainingSize;
                _logger.LogInformation("Partial close | Qty: {Qty}", reduceQty);
                await _executor.ReducePositionAsync(new ExecutionRequest
                {
                    AccountId = acc.Config.AccountId,
                    ApiKey = acc.Config.ApiKey,
                    ApiSecret = acc.Config.ApiSecret,
                    Direction = trade.Direction
                }, reduceQty);
            };

            _positionManager.StopLossUpdateRequested += async trade =>
            {
                if (trade.AccountId == "SIM")
                    return;

                var acc = _accounts.Accounts.First(x => x.Config.AccountId == trade.AccountId);
                var newSl = trade.StopLoss;

                // Prevent invalid SL (Bybit rejects silently sometimes)
                if (trade.Direction == SignalDirection.Long && newSl >= _currentMarketPrice)
                {
                    _logger.LogWarning("Invalid SL for LONG, skipping update");
                    return;
                }

                if (trade.Direction == SignalDirection.Short && newSl <= _currentMarketPrice)
                {
                    _logger.LogWarning("Invalid SL for SHORT, skipping update");
                    return;
                }
                await _executor.UpdateStopLossAsync(
                    new ExecutionRequest
                    {
                        AccountId = acc.Config.AccountId,
                        ApiKey = acc.Config.ApiKey,
                        ApiSecret = acc.Config.ApiSecret,
                        Direction = trade.Direction,
                        Quantity = trade.RemainingSize
                    },
                    trade.StopLoss);
            };
            _positionManager.FullCloseRequested += async trade =>
            {
                if (trade.AccountId == "SIM")
                    return;

                var acc = _accounts.Accounts.First(x => x.Config.AccountId == trade.AccountId);

                await _executor.ClosePositionAsync(new ExecutionRequest
                {
                    AccountId = acc.Config.AccountId,
                    ApiKey = acc.Config.ApiKey,
                    ApiSecret = acc.Config.ApiSecret,
                    Direction = trade.Direction,
                    Quantity = trade.RemainingSize
                });
            };

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bootstrap.InitializeAsync();
            await _depthClient.StartAsync(stoppingToken);
            await _tradeStreamClient.StartAsync(stoppingToken);
            await _depthStreamClient.StartAsync(stoppingToken);
            await _depthFeatureManager.LoadAsync(stoppingToken);
            _logger.LogInformation("Depth Features Loaded | Rows:{Rows}", _depthFeatureManager.Rows.Count);

            var persisted = _positionPersistence.Load();
            if (persisted.Any())
            {
                _positionManager.Restore(persisted);
                _logger.LogInformation("Restored {count} open positions", persisted.Count);
            }

            // ---- RESTORE PROP ACCOUNT STATE ----
            foreach (var acc in _accounts.Accounts)
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
            if ((DateTime.UtcNow - _lastSync).TotalSeconds > 5)
            {
                await _executionSync.SyncAsync();
                _lastSync = DateTime.UtcNow;
            }

            var snapshot = _marketStateManager.Load();
            if (snapshot != null)
            {
                _scoreNorm.Restore(snapshot.ScoreNormalizer);
                _velNorm.Restore(snapshot.VelocityNormalizer);
                _imbNorm.Restore(snapshot.ImbalanceNormalizer);
                _exhNorm.Restore(snapshot.ExhaustionNormalizer);
                _cmpNorm.Restore(snapshot.CompressionNormalizer);
                _adjustmentNorm.Restore(snapshot.AdjustedScoreNormalizer);

                if (snapshot.MomentumRun == 0)
                {
                    _momentumRun.Bootstrap(0); // neutral start
                }
                else
                    _momentumRun.Restore(snapshot.MomentumRun);

                _priceEngine.Restore(snapshot.PriceFastEma, snapshot.PriceSlowEma);
                _scoreEngine.Restore(snapshot.ScoreFastEma, snapshot.ScoreSlowEma, snapshot.ScoreMediumEma);
                _probEngine.Restore(snapshot.ProbFastEma, snapshot.ProbSlowEma, snapshot.ProbMediumEma);
                if (snapshot.VolumeEngineState != null)
                {
                    _volumeEngine.Restore(snapshot.VolumeEngineState);
                    _logger.LogInformation("VolumeEngine state restored successfully.");
                }
                if (snapshot.EmaStabilityState != null)
                    _emaStability.Restore(snapshot.EmaStabilityState);

                if (snapshot.AtrNormalizer != null)
                    _atrNormalizer.Restore(snapshot.AtrNormalizer);

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
                _currentMarketPrice = (decimal)price;

                var depth = _depthStreamClient.CurrentSnapshot;
                var feature = _depthFeatureEngine.Calculate(depth);
                _depthMinuteAggregator.Add(feature);
                _depthWallTracker.Update(depth);

                if (_depthMinuteAggregator.IsReady() && DateTime.UtcNow - _lastDepthLogUtc >= TimeSpan.FromMinutes(1))
                {
                    var minute = _depthMinuteAggregator.Build();
                    var topBidWalls = _depthWallTracker.GetTopBidWalls();
                    var topAskWalls = _depthWallTracker.GetTopAskWalls();

                    var depthFeature = new DepthMinuteFeature
                    {
                        TimestampUtc = DateTime.UtcNow,
                        Price = (decimal)price,
                        PriceChange1m = minute.PriceChange1m,
                        AvgImbalance10 = minute.AverageImbalanceTop10,
                        BullishPercent = minute.BullishBookPercent,
                        BearishPercent = minute.BearishBookPercent,
                        BullishPersistenceSeconds = minute.BullishPersistenceSeconds,
                        BearishPersistenceSeconds = minute.BearishPersistenceSeconds,
                        LargestBidWallAgeSec = topBidWalls.Any() ? topBidWalls.Max(x => (x.LastSeenUtc - x.FirstSeenUtc).TotalSeconds) : 0,
                        LargestAskWallAgeSec = topAskWalls.Any() ? topAskWalls.Max(x => (x.LastSeenUtc - x.FirstSeenUtc).TotalSeconds) : 0,
                        LargestBidWallQty = topBidWalls.Any() ? topBidWalls.Max(x => x.MaxQuantitySeen) : 0,
                        LargestAskWallQty = topAskWalls.Any() ? topAskWalls.Max(x => x.MaxQuantitySeen) : 0,
                        ConsumedBidWallCount = topBidWalls.Count(x => x.IsConsumed),
                        ConsumedAskWallCount = topAskWalls.Count(x => x.IsConsumed),
                        AvgBidQuantityChangePct = topBidWalls.Any() ? topBidWalls.Average(x => x.QuantityChangePercent) : 0,
                        AvgAskQuantityChangePct = topAskWalls.Any() ? topAskWalls.Average(x => x.QuantityChangePercent) : 0
                    };

                    _depthFeatureManager.Add(depthFeature);
                    await _depthFeatureManager.SaveAsync(stoppingToken);
                    var mtf = new DepthMtfAggregator(_depthFeatureManager.Rows).Build();
                    _lastDepthLogUtc = DateTime.UtcNow;
                }
                if ((DateTime.UtcNow - _lastOiFetch).TotalSeconds > 120)
                {
                    _lastOiValue = await _openInterestClient.GetOpenInterestAsync();
                    _lastOiFetch = DateTime.UtcNow;
                }
                // === FUNDING RATE (new) ===
                double currentFundingRate = 0;
                if ((DateTime.UtcNow.Second % 30) == 0) // every 30 seconds
                {
                    currentFundingRate = await _fundingRateClient.GetCurrentFundingRateAsync();
                    _fundingRateEngine.Update(currentFundingRate);
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
                    result = _engineProcessor.Process(price, lastTrade.Quantity, lastTrade.Timestamp, _lastOiValue, _fundingRateEngine.CurrentRate, _fundingRateEngine.FundingPressure);
                    _strengthEngine.Update(result);

                    if (DateTime.UtcNow - _lastMarketStateSaveUtc >= TimeSpan.FromMinutes(1))
                    {
                        _gptMarketStateManager.Add(CreateMarketStateRow(result));

                        await _gptMarketStateManager.SaveAsync(stoppingToken);

                        _lastMarketStateSaveUtc = DateTime.UtcNow;
                    }
                    if (DateTime.UtcNow - _lastGptReviewUtc >= TimeSpan.FromMinutes(15) && _gptMarketStateManager.Rows.Count >= 120 && _depthFeatureManager.Rows.Count >= 120)
                    {
                        var sequence = _gptSessionManager.GetNextSequence();
                        var snapshotV2 = _snapshotV2Builder.Build(sequence, EngineVersion.Version, result);
                        _gptReviewQueue.Enqueue(snapshotV2);
                        _lastGptReviewUtc = DateTime.UtcNow;
                        _logger.LogInformation("GPT Review Queued | Seq:{Seq}", sequence);
                    }
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

                if (IsStabilizationPeriodOver())
                {
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
                                var acc = _accounts.Accounts.FirstOrDefault(x => x.Config.AccountId == trade.AccountId);

                                if (acc != null && trade.NotifyThroughTelegram)
                                {
                                    await _alerts.SendExitAsync(trade, exitPrice, pnl, 0m, acc.State.CurrentEquity, acc.State.DailyDrawdownPct, acc.State.OverallDrawdownPct);
                                }
                            }
                            continue;
                        }
                    }

                    // ---------- ENTRY SECOND ----------
                    var signals = _strategyEngine.Evaluate(result);
                    foreach (var signal in signals)
                    {
                        if (signal.Direction == SignalDirection.None)
                            continue;

                        if (!ShouldAllowEntry(result))
                            continue;

                        bool isFairPrice = signal.Direction == SignalDirection.Long ?
                            _fairPriceEngine.IsFairLongEntry((decimal)price, result.VWAP, result.ATR) :
                            _fairPriceEngine.IsFairShortEntry((decimal)price, result.VWAP, result.ATR);

                        bool isVolumeExpansion = _volumeEngine.IsVolumeExpansion();

                        if (!IsReentryAllowed(signal, result, (decimal)price, isFairPrice, isVolumeExpansion))
                            continue;

                        _router.Route(signal, (decimal)result.Price, (decimal)result.ATR15m, result);

                        if (signal.NotifyThroughTelegram)
                        {
                            var newTrades = _positionManager.GetAllPositions()
                                .Where(t => t.StrategyName == signal.StrategyName &&
                                            !t.EntryAlertSent &&
                                            !t.Closed);

                            foreach (var trade in newTrades)
                            {
                                var acc = _accounts.Accounts.FirstOrDefault(x => x.Config.AccountId == trade.AccountId);

                                if (acc != null)
                                {
                                    await _alerts.SendEntryAsync(
                                        trade,
                                        acc.State.CurrentEquity,
                                        acc.State.DailyDrawdownPct,
                                        acc.State.OverallDrawdownPct);
                                }

                                trade.EntryAlertSent = true;
                            }
                        }
                    }
                    _positionManager.Update((decimal)price);
                    _equityEngine.UpdateAll((decimal)price);
                }

                var fairLongPrice = _fairPriceEngine.GetFairLongPrice(result.VWAP, result.ATR);
                var fairShortPrice = _fairPriceEngine.GetFairShortPrice(result.VWAP, result.ATR);

                var maxLong = _fairPriceEngine.GetMaxChaseLongPrice(result.VWAP, result.ATR);
                var maxShort = _fairPriceEngine.GetMaxChaseShortPrice(result.VWAP, result.ATR);

                _lastFlowSnapshot = string.Format(
                                            "FLOW | P {0:F2} Raw {1:F2} Adj {2:F2} Sz {3:F2} VzEma {4:F2} ProbL:{5:F2} | " +
                                            "FS {6:F2} MS {7:F2} SS {8:F2} | VWAP {9:F2} ER5 {10:F2} ER30 {11:F2} ATR {12:F2} " +
                                            "| B {13:F2} S {14:F2} Net {15:F2} | FP {16:F4} MP {17:F4} SP {18:F4} | " +
                                            "v15 {19:F2} v1 {20:F2} F {21:F3} | " +
                                            "SF[{22:F4},{23:F4},{24:F4}] {25} | " +
                                            "SM[{26:F4},{27:F4},{28:F4}] {29} | " +
                                            "SS[{30:F4},{31:F4},{32:F4}] {33} | " +
                                            "PF[{34:F4},{35:F4},{36:F4}] {37} | " +
                                            "PM[{38:F4},{39:F4},{40:F4}] {41} | " +
                                            "PS[{42:F4},{43:F4},{44:F4}] {45} | " +
                                            "FR {46:F6} FP {47:F6} | atrN {48:F2} atrS {49:F2} reg {50:F2} | L1 {51} L2 {52} L3 {53} | FairL:{54:F2} FairS:{55:F2} MaxL:{56:F2} MaxS:{57:F2} flip:{58:F2}" +
                                            " | FEMA {59} MEMA {60} SEMA {61} | Strength:{62:F2} Delta:{63:F4} \n",

                                            result.Price, result.RawScore, result.AdjustedScore, result.ScoreZ, result.VelocityEma,
                                            result.LongProbability, result.ScoreFastEma, result.ScoreMediumEma, result.ScoreSlowEma,
                                            result.VWAP, result.ER, result.ER30, result.ATR15m,
                                            result.BuyPressure, result.SellPressure, result.NetPressure,
                                            result.ProbFastEma, result.ProbMediumEma, result.ProbSlowEma,
                                            result.Volume15, result.Volume1, result.TrendFactor,

                                            result.EmaStability.ScoreFastEmaLevel1, result.EmaStability.ScoreFastEmaLevel2, result.EmaStability.ScoreFastEmaLevel3, result.EmaStability.FastScoreTrend,
                                            result.EmaStability.ScoreMediumEmaLevel1, result.EmaStability.ScoreMediumEmaLevel2, result.EmaStability.ScoreMediumEmaLevel3, result.EmaStability.MediumScoreTrend,
                                            result.EmaStability.ScoreSlowEmaLevel1, result.EmaStability.ScoreSlowEmaLevel2, result.EmaStability.ScoreSlowEmaLevel3, result.EmaStability.SlowScoreTrend,

                                            result.EmaStability.ProbFastEmaLevel1, result.EmaStability.ProbFastEmaLevel2, result.EmaStability.ProbFastEmaLevel3, result.EmaStability.FastProbTrend,
                                            result.EmaStability.ProbMediumEmaLevel1, result.EmaStability.ProbMediumEmaLevel2, result.EmaStability.ProbMediumEmaLevel3, result.EmaStability.MediumProbTrend,
                                            result.EmaStability.ProbSlowEmaLevel1, result.EmaStability.ProbSlowEmaLevel2, result.EmaStability.ProbSlowEmaLevel3, result.EmaStability.SlowProbTrend,

                                            result.FundingRate, result.FundingPressure, result.AtrNorm, result.AtrScale, result.EmaStability.Regime,
                                            result.EmaStability.Level1, result.EmaStability.Level2, result.EmaStability.Level3, fairLongPrice, fairShortPrice,
                                            maxLong, maxShort, result.EmaStability.Flip, result.FastEmaPeriod, result.MediumEmaPeriod, result.SlowEmaPeriod, _strengthEngine.LastStrength, _strengthEngine.Delta);
                _logger.LogInformation(_lastFlowSnapshot);


                var now = DateTime.UtcNow;

                if ((now - _lastPositionLog).TotalMinutes >= 10)
                {
                    var positions = _positionManager.GetAllPositions();
                    var positionText = BuildPositionSummary(positions, (decimal)result.Price);
                    var message = $"📊 *Kinetix Update*\n\n" + $"`{_lastFlowSnapshot}`\n\n" + $"📌 *Positions*\n{positionText}";
                    _logger.LogInformation(positionText);
                    _ = _notificationService.SendMessageAsync(message);
                    _lastPositionLog = now;
                }
                _recorder.Record(result);

                if ((DateTime.UtcNow - _lastSnapshot).TotalSeconds > 60)
                {

                    foreach (var acc in _accounts.Accounts)
                    {
                        var client = _factory.GetClient(acc.Config.AccountId, acc.Config.ApiKey, acc.Config.ApiSecret);
                        decimal realBalance = await client.GetUsdtWalletBalanceAsync();

                        if (realBalance > 0 && Math.Abs(realBalance - acc.State.CurrentEquity) > 0.01m)
                        {
                            _logger.LogInformation("Periodic balance sync | {Account} | Adjusted {Old} → {New}",
                                acc.Config.AccountId, acc.State.CurrentEquity, realBalance);

                            acc.State.CurrentEquity = realBalance;
                            acc.State.LastBalanceSyncUtc = DateTime.UtcNow;
                        }
                        _accountStatePersistence.Update(acc.Config.AccountId, acc.State);
                    }
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
                        CompressionNormalizer = _cmpNorm.GetState(),
                        AdjustedScoreNormalizer = _adjustmentNorm.GetState(),
                        VolumeEngineState = _volumeEngine.GetState(),

                        EmaStabilityState = result.EmaStability,
                        AtrNormalizer = _atrNormalizer.GetState()
                    });

                    _lastSnapshot = DateTime.UtcNow;
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        private bool ShouldAllowEntry(KinetixEngineResult result)
        {
            // Example global gate: only allow entries if ATR15m is above a certain threshold
            if (result.ATR15m < 100 && result.AtrNorm < .25)
            {
                //_logger.LogInformation("Global entry gate: ATR15m too low ({ATR15m}), skipping entry evaluation", result.ATR15m);
                return false;
            }
            return true;
        }

        private string BuildPositionSummary(IEnumerable<ActiveTrade> positions, decimal currentPrice)
        {
            if (!positions.Any()) return "No open positions";

            var sb = new StringBuilder();

            foreach (var p in positions)
            {
                decimal pnl = p.Direction == SignalDirection.Long
                    ? currentPrice - p.EntryPrice
                    : p.EntryPrice - currentPrice;

                sb.AppendLine($"{p.StrategyName} | {p.Direction} | Entry:{p.EntryPrice:F2} | PnL:{pnl:F2}");
            }

            return sb.ToString();
        }

        private bool IsReentryAllowed(StrategySignal signal, KinetixEngineResult r, decimal price, bool isFairPrice, bool isVolumeExpansion)
        {
            //Naveen need to make it account wise
            // No previous trade → allow
            var last = _tradeMemory.Get(signal.StrategyName);
            if (last == null)
                return true;

            if ((DateTime.UtcNow - last.ExitTime).TotalMinutes < 2)
                return false;

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

        private bool IsStabilizationPeriodOver()
        {
            var now = DateTime.UtcNow;
            var minutesSinceStart = (now - _engineStartTimeUtc).TotalMinutes;

            // Option A: simple fixed delay after start
            if (minutesSinceStart < _options.StabilizationMinutesBeforeTrading)
            {
                _logger.LogInformation("Engine in stabilization phase ({Minutes:F2}/{Required} min elapsed)",
                    minutesSinceStart, _options.StabilizationMinutesBeforeTrading);
                return false;
            }

            return true;
        }

        private GptMarketStateRow CreateMarketStateRow(KinetixEngineResult result)
        {
            return new GptMarketStateRow
            {
                TimestampUtc = DateTime.UtcNow,

                ScoreZ = result.ScoreZ,

                VelocityZ = result.VelocityZ,

                ImbalanceZ = result.ImbalanceZ,

                CompressionZ = result.CompressionZ,

                ExhaustionZ = result.ExhaustionZ,

                Momentum = result.Momentum,

                Acceleration = result.Acceleration,

                Persistence = result.Persistence,

                NetPressure = result.NetPressure,

                OIChange = result.OIChange,

                FundingPressure = result.FundingPressure,

                FlowImpactEfficiency =
                    result.FlowImpactEfficiency,

                ER5 = result.ER5,

                ER30 = result.ER30
            };
        }

    }
}
