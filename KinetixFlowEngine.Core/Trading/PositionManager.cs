using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Execution;
using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Utils;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;

namespace KinetixFlowEngine.Core.Trading
{
    public class PositionManager
    {
        private readonly PositionPersistence _persistence;

        public event Action<ActiveTrade>? Target1Reached;
        public event Action<ActiveTrade, decimal>? TradeClosed;
        private DateTime _lastSave = DateTime.MinValue;
        private readonly Dictionary<string, ActiveTrade> _activeTrades = new();
        public event Action<ActiveTrade>? PartialCloseRequested;
        public event Action<ActiveTrade>? StopLossUpdateRequested;
        private readonly StrategyConfigLoader _strategyConfigLoader;
        private readonly ILogger<PositionManager> _logger;
        private readonly FairPriceEngine _fairPriceEngine;
        private readonly QuantConsensusIntentTracker _intentTracker;
        public event Action<ActiveTrade>? FullCloseRequested;
        private readonly ConcurrentDictionary<string, bool> _closingInProgress = new();

        private static string GetKey(string strategy, string accountId)
    => $"{accountId}::{strategy}";

        public PositionManager(
            PositionPersistence persistence,
            StrategyConfigLoader strategyConfigLoader,
            FairPriceEngine fairPriceEngine,
            QuantConsensusIntentTracker intentTracker,
            ILogger<PositionManager> logger)
        {
            _persistence = persistence;
            _activeTrades = new Dictionary<string, ActiveTrade>();
            _strategyConfigLoader = strategyConfigLoader;
            _fairPriceEngine = fairPriceEngine;
            _intentTracker = intentTracker;
            _logger = logger;
        }

        public async Task<bool> HasPosition(string strategy, string accountId)
        {
            return _activeTrades.ContainsKey(GetKey(strategy, accountId));
        }

        public ActiveTrade? GetPosition(string strategy, string accountId)
        {
            _activeTrades.TryGetValue(GetKey(strategy, accountId), out var trade);
            return trade;
        }

        public IEnumerable<ActiveTrade> GetAllPositions()
        {
            return _activeTrades.Values;
        }

        public async Task TryEnterTrade(StrategySignal signal, decimal price, double atr, KinetixEngineResult r, decimal size, string accountId, string orderId)
        {
            var key = GetKey(signal.StrategyName, accountId);
            if (_activeTrades.ContainsKey(key))
                return;

            var config = _strategyConfigLoader.Get(signal.StrategyName);
            decimal atrValue = Math.Max(0m, (decimal)atr);
            decimal stopDistance = CalculateDistance(
                price,
                atrValue,
                config.StopLossAtr,
                config.StopLossPercent,
                price * 0.01m);

            decimal target1Distance = CalculateDistance(
                price,
                atrValue,
                config.Target1Atr,
                config.Target1Percent,
                stopDistance);

            decimal target1SizePercent = SanitizePercent(config.Target1SizePercent);
            decimal configuredLeverage = config.Leverage > 0m ? config.Leverage : 1m;

            var entryUtc = DateTimeOffset.UtcNow;
            var fairPriceAtEntry = signal.Direction == SignalDirection.Long
                ? _fairPriceEngine.GetFairLongPrice(r.VWAP, r.ATR)
                : _fairPriceEngine.GetFairShortPrice(r.VWAP, r.ATR);

            var trade = new ActiveTrade
            {
                AccountId = accountId,
                StrategyName = signal.StrategyName,
                Direction = signal.Direction,
                EntryPrice = price,
                StopLoss = signal.Direction == SignalDirection.Long
                    ? price - stopDistance
                    : price + stopDistance,

                Target1 = signal.Direction == SignalDirection.Long
                    ? price + target1Distance
                    : price - target1Distance,

                Target1SizePercent = target1SizePercent,
                ConfiguredLeverage = configuredLeverage,
                TrailingStop = stopDistance,
                InitialSize = size,
                RemainingSize = size,

                EntryTimeMs = entryUtc.ToUnixTimeMilliseconds(),
                EntryUtc = entryUtc,
                NotifyThroughTelegram = signal.NotifyThroughTelegram,

                MaxPrice = price,
                MinPrice = price,

                EntryScoreZ = r?.ScoreZ ?? 0,
                EntryVelocityZ = r?.VelocityZ ?? 0,
                EntryImbalanceZ = r?.ImbalanceZ ?? 0,
                EntryCompressionZ = r?.CompressionZ ?? 0,
                EntryATR = r?.ATR ?? 0,
                EntryER = r?.ER ?? 0,
                EntryFlowState = r?.FlowState.State.ToString() ?? "Unknown",
                Closed = false,
                EntryPriceTrend = (double)r.PriceTrend,
                EntryScoreTrend = (double)r.ScoreTrend,
                OrderId = orderId,

                QuantIntentId = signal.QuantIntentId,
                CurrentPayloadId = signal.CurrentPayloadId,
                PreviousPayloadId = signal.PreviousPayloadId,
                ThirdPayloadId = signal.ThirdPayloadId,
                ConsensusDecisionUtc = signal.ConsensusDecisionUtc,
                SignalUtc = signal.SignalUtc,
                PendingIntentCreatedUtc = signal.PendingIntentCreatedUtc,
                ReviewCount = signal.ReviewCount,
                CurrentBatchScore = signal.CurrentBatchScore,
                TemporalScore = signal.TemporalScore,
                ExecutableVotes = signal.ExecutableVotes,
                DirectionalAgreement = signal.DirectionalAgreement,
                ExecutableAgreement = signal.ExecutableAgreement,
                ExecutableBatchCount = signal.ExecutableBatchCount,
                ReviewSpanMinutes = signal.ReviewSpanMinutes,
                MarketPriceAtSignal = signal.MarketPriceAtSignal,
                FairPriceAtSignal = signal.FairPriceAtSignal,
                FairPriceAtEntry = fairPriceAtEntry,
                EntryDelaySeconds = signal.SignalUtc.HasValue
                    ? Math.Max(0, (entryUtc - signal.SignalUtc.Value).TotalSeconds)
                    : 0
            };

            _activeTrades[key] = trade;

            _intentTracker.MarkExecuted(
                signal,
                accountId,
                orderId,
                entryUtc,
                price,
                fairPriceAtEntry);

            SaveThrottled();
        }

        public async Task Update(decimal price)
        {
            var trades = _activeTrades.Values.ToList();

            foreach (var trade in trades)
            {
                if (trade.Closed)
                    continue;

                trade.MaxPrice = Math.Max(trade.MaxPrice, price);
                trade.MinPrice = Math.Min(trade.MinPrice, price);

                if (!trade.Target1Hit)
                {
                    bool hit = false;

                    if (trade.Direction == SignalDirection.Long && price >= trade.Target1)
                        hit = true;

                    if (trade.Direction == SignalDirection.Short && price <= trade.Target1)
                        hit = true;

                    if (hit)
                    {
                        trade.Target1Hit = true;
                        Target1Reached?.Invoke(trade);

                        decimal target1ExitPercent = trade.Target1SizePercent > 0m
                            ? SanitizePercent(trade.Target1SizePercent)
                            : SanitizePercent(_strategyConfigLoader.Get(trade.StrategyName).Target1SizePercent);

                        if (target1ExitPercent >= 100m)
                        {
                            _logger.LogInformation(
                                "Trade {Strategy} on account {AccountId} hit Target 1 FULL close. Price={Price} Target1={Target1}",
                                trade.StrategyName,
                                trade.AccountId,
                                price,
                                trade.Target1);

                            await CloseTrade(
                                trade.StrategyName,
                                trade.AccountId,
                                price,
                                "TP1_FULL");

                            continue;
                        }

                        decimal sanitizedQty = Math.Round(
                            trade.InitialSize - (trade.InitialSize * target1ExitPercent / 100m),
                            3,
                            MidpointRounding.ToZero);

                        trade.RemainingSize = Math.Max(0m, sanitizedQty);

                        _logger.LogInformation(
                            "Trade {Strategy} on account {AccountId} hit Target 1 PARTIAL. ExitPercent={ExitPercent} RemainingSize={RemainingSize}",
                            trade.StrategyName,
                            trade.AccountId,
                            target1ExitPercent,
                            trade.RemainingSize);

                        PartialCloseRequested?.Invoke(trade);

                        if (!trade.MovedToBreakeven)
                        {
                            decimal buffer = trade.EntryPrice * 0.0002m;

                            trade.StopLoss = trade.Direction == SignalDirection.Long
                                ? trade.EntryPrice + buffer
                                : trade.EntryPrice - buffer;

                            trade.MovedToBreakeven = true;

                            StopLossUpdateRequested?.Invoke(trade);
                        }
                    }
                }

                if (trade.Direction == SignalDirection.Long && price <= trade.StopLoss)
                {
                    await CloseTrade(trade.StrategyName, trade.AccountId, price, trade.Target1Hit ? "TSL" : "SL");
                }

                if (trade.Direction == SignalDirection.Short && price >= trade.StopLoss)
                {
                    await CloseTrade(trade.StrategyName, trade.AccountId, price, trade.Target1Hit ? "TSL" : "SL");
                }
            }

            SaveThrottled();
        }

        public async Task CloseTrade(string strategyName, string accountId, decimal exitPrice, string reason = "SL")
        {
            var key = GetKey(strategyName, accountId);

            if (!_activeTrades.TryGetValue(key, out var trade))
                return;

            if (trade.Closed)
                return;

            _closingInProgress.TryAdd(key, true);
            FullCloseRequested?.Invoke(trade);

            trade.ExitReason = reason;
            trade.Closed = true;
            _activeTrades.Remove(key);

            SaveThrottled();
            TradeClosed?.Invoke(trade, exitPrice);
        }


        private static decimal CalculateDistance(
            decimal entryPrice,
            decimal atr,
            decimal atrMultiple,
            decimal percent,
            decimal fallback)
        {
            var distances = new List<decimal>();

            if (atr > 0m && atrMultiple > 0m)
                distances.Add(atr * atrMultiple);

            if (entryPrice > 0m && percent > 0m)
                distances.Add(entryPrice * percent / 100m);

            if (distances.Count == 0)
                return Math.Max(0m, fallback);

            return distances.Min();
        }

        private static decimal SanitizePercent(decimal percent)
        {
            if (percent < 0m)
                return 0m;

            if (percent > 100m)
                return 100m;

            return percent;
        }

        private async Task SaveThrottled()
        {
            if ((DateTime.UtcNow - _lastSave).TotalSeconds < 5)
                return;

            _persistence.Save(GetAllPositions());
            _lastSave = DateTime.UtcNow;
        }

        public async Task Restore(IEnumerable<PersistedPosition> persisted)
        {
            foreach (var p in persisted)
            {
                var trade = new ActiveTrade
                {
                    AccountId = p.AccountId,
                    StrategyName = p.StrategyName,
                    Direction = p.Direction,
                    EntryPrice = p.EntryPrice,
                    StopLoss = p.StopLoss,
                    Target1 = p.Target1,
                    Target1SizePercent = p.Target1SizePercent,
                    ConfiguredLeverage = p.ConfiguredLeverage > 0m ? p.ConfiguredLeverage : 1m,
                    InitialSize = p.Size,
                    RemainingSize = p.RemainingSize,   // ✅ FIX
                    TrailingStop = p.TrailingStop,     // ✅ FIX

                    EntryTimeMs = p.EntryTimeMs,
                    Target1Hit = p.Target1Hit,
                    MovedToBreakeven = p.MovedToBreakeven, // ✅ FIX

                    MaxPrice = p.MaxPrice,
                    MinPrice = p.MinPrice,

                    EntryScoreZ = p.EntryScoreZ,
                    EntryVelocityZ = p.EntryVelocityZ,
                    EntryImbalanceZ = p.EntryImbalanceZ,
                    EntryCompressionZ = p.EntryCompressionZ,
                    EntryATR = p.EntryATR,
                    EntryER = p.EntryER,
                    EntryFlowState = p.EntryFlowState,

                    EntryAlertSent = p.EntryAlertSent,
                    EntryPriceTrend = p.EntryPriceTrend,
                    EntryScoreTrend = p.EntryScoreTrend,
                    OrderId = p.OrderId,

                    QuantIntentId = p.QuantIntentId,
                    CurrentPayloadId = p.CurrentPayloadId,
                    PreviousPayloadId = p.PreviousPayloadId,
                    ThirdPayloadId = p.ThirdPayloadId,
                    ConsensusDecisionUtc = p.ConsensusDecisionUtc,
                    SignalUtc = p.SignalUtc,
                    PendingIntentCreatedUtc = p.PendingIntentCreatedUtc,
                    EntryUtc = p.EntryUtc,
                    ReviewCount = p.ReviewCount,
                    CurrentBatchScore = p.CurrentBatchScore,
                    TemporalScore = p.TemporalScore,
                    ExecutableVotes = p.ExecutableVotes,
                    DirectionalAgreement = p.DirectionalAgreement,
                    ExecutableAgreement = p.ExecutableAgreement,
                    ExecutableBatchCount = p.ExecutableBatchCount,
                    ReviewSpanMinutes = p.ReviewSpanMinutes,
                    MarketPriceAtSignal = p.MarketPriceAtSignal,
                    FairPriceAtSignal = p.FairPriceAtSignal,
                    FairPriceAtEntry = p.FairPriceAtEntry,
                    EntryDelaySeconds = p.EntryDelaySeconds,
                    IntentExpiryReason = p.IntentExpiryReason,
                    Closed = false
                };

                var key = GetKey(trade.StrategyName, trade.AccountId);
                _activeTrades[key] = trade;
            }
        }

        public async Task RestoreFromExchange(ExchangePosition ex)
        {
            //NaveenImp - need to make wayy to have strategy name when we dont have positions in json locally, but position open in bybit
            var trade = new ActiveTrade
            {
                OrderId = ex.OrderId,
                EntryPrice = ex.EntryPrice,
                InitialSize = ex.Quantity,
                RemainingSize = ex.Quantity,
                AccountId = ex.AccountId,
                StrategyName = "Recovered",
                Direction = SignalDirection.Long, // temporary
                EntryTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Closed = false,
                MaxPrice = ex.EntryPrice,
                MinPrice = ex.EntryPrice
            };

            var key = GetKey(trade.StrategyName + "_" + trade.OrderId, trade.AccountId);
            _activeTrades[key] = trade;

        }

        public async Task ForceRemoveRemnantByPrice(string accountId, decimal entryPrice, decimal tinyQty)
        {
            var trade = _activeTrades.Values.FirstOrDefault(t =>
                t.AccountId == accountId &&
                Math.Abs(t.EntryPrice - entryPrice) < 2 &&
                Math.Abs(t.RemainingSize - tinyQty) < 0.002m);

            if (trade != null)
            {
                _activeTrades.Remove(GetKey(trade.StrategyName, accountId));
                TradeClosed?.Invoke(trade, trade.EntryPrice); // trigger PnL journal etc.
                _logger.LogInformation("Force-removed remnant {Strategy} on {Account} (qty {Qty})",
                    trade.StrategyName, accountId, tinyQty);
            }
        }

    }
}