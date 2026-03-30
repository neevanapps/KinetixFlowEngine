using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Execution;
using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Strategy;
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
        public event Action<ActiveTrade>? FullCloseRequested;
        private readonly ConcurrentDictionary<string, bool> _closingInProgress = new();

        private static string GetKey(string strategy, string accountId)
    => $"{accountId}::{strategy}";

        public PositionManager(PositionPersistence persistence, StrategyConfigLoader strategyConfigLoader, ILogger<PositionManager> logger)
        {
            _persistence = persistence;
            _activeTrades = new Dictionary<string, ActiveTrade>();
            _strategyConfigLoader = strategyConfigLoader;
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

            decimal atrValue = (decimal)atr;
            decimal stopDistance = Math.Min(price * 0.01m, atrValue * 3);

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
                    ? price + stopDistance
                    : price - stopDistance,

                TrailingStop = stopDistance,
                InitialSize = size,
                RemainingSize = size,

                EntryTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
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
                OrderId = orderId
            };

            _activeTrades[key] = trade;

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

                        //var reduceQty = trade.InitialSize * 0.7m;
                        var config = _strategyConfigLoader.Get(trade.StrategyName);
                        decimal sanitizedQty = Math.Round(trade.InitialSize - (trade.InitialSize * config.Target1SizePercent / 100), 3, MidpointRounding.ToZero);
                        trade.RemainingSize =sanitizedQty;

                        _logger.LogInformation("NaveenImp: Trade {Strategy} on account {AccountId} hit Target 1. Remaining size: {RemainingSize}",
                            trade.StrategyName, trade.AccountId, trade.RemainingSize);

                        // 🔥 EVENT instead of direct execution
                        PartialCloseRequested?.Invoke(trade);

                        if (!trade.MovedToBreakeven)
                        {
                            decimal buffer = trade.EntryPrice * 0.0002m;

                            trade.StopLoss = trade.Direction == SignalDirection.Long
                                ? trade.EntryPrice + buffer
                                : trade.EntryPrice - buffer;

                            trade.MovedToBreakeven = true;

                            // 🔥 EVENT instead of executor call
                            StopLossUpdateRequested?.Invoke(trade);
                        }

                        Target1Reached?.Invoke(trade);
                    }
                }

                if (trade.Direction == SignalDirection.Long && price <= trade.StopLoss)
                {
                    CloseTrade(trade.StrategyName, trade.AccountId, price, trade.Target1Hit ? "TSL" : "SL");
                }

                if (trade.Direction == SignalDirection.Short && price >= trade.StopLoss)
                {
                    CloseTrade(trade.StrategyName, trade.AccountId, price, trade.Target1Hit ? "TSL" : "SL");
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

                    EntryAlertSent = p.EntryAlertSent, // ✅ FIX
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