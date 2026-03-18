using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Strategy;
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
        private static string GetKey(string strategy, string accountId)
    => $"{accountId}::{strategy}";

        public PositionManager(PositionPersistence persistence)
        {
            _persistence = persistence;
        }

        public bool HasPosition(string strategy, string accountId)
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

        public void TryEnterTrade(StrategySignal signal, decimal price, double atr, KinetixEngineResult r, decimal size, string accountId)
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
            };

            _activeTrades[key] = trade;

            SaveThrottled();
        }

        public void Update(decimal price)
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
                        trade.RemainingSize *= 0.3m;

                        if (!trade.MovedToBreakeven)
                        {
                            decimal buffer = trade.EntryPrice * 0.0002m; // ~0.02% buffer (optional)

                            if (trade.Direction == SignalDirection.Long)
                                trade.StopLoss = trade.EntryPrice + buffer;
                            else
                                trade.StopLoss = trade.EntryPrice - buffer;

                            trade.MovedToBreakeven = true;
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

        public void CloseTrade(string strategyName, string accountId, decimal exitPrice, string reason = "SL")
        {
            var key = GetKey(strategyName, accountId);

            if (!_activeTrades.TryGetValue(key, out var trade))
                return;

            if (trade.Closed)
                return;

            trade.Closed = true;
            trade.ExitReason = reason;

            _activeTrades.Remove(key);

            SaveThrottled();
            TradeClosed?.Invoke(trade, exitPrice);
        }

        private void SaveThrottled()
        {
            if ((DateTime.UtcNow - _lastSave).TotalSeconds < 5)
                return;

            _persistence.Save(GetAllPositions());
            _lastSave = DateTime.UtcNow;
        }

        public void Restore(IEnumerable<PersistedPosition> persisted)
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
                    RemainingSize = p.Size * (p.Target1Hit ? 0.3m : 1m),
                    EntryTimeMs = p.EntryTimeMs,
                    Target1Hit = p.Target1Hit,
                    MaxPrice = p.MaxPrice,
                    MinPrice = p.MinPrice,
                    EntryScoreZ = p.EntryScoreZ,
                    EntryVelocityZ = p.EntryVelocityZ,
                    EntryImbalanceZ = p.EntryImbalanceZ,
                    EntryCompressionZ = p.EntryCompressionZ,
                    EntryATR = p.EntryATR,
                    EntryER = p.EntryER,
                    EntryFlowState = p.EntryFlowState,
                    NotifyThroughTelegram = true, // safe default
                    Closed = false,
                    TrailingStop = Math.Abs(p.EntryPrice - p.StopLoss),
                    MovedToBreakeven = p.Target1Hit,
                    EntryPriceTrend = p.EntryPriceTrend,
                    EntryScoreTrend = p.EntryScoreTrend,
                };

                AddRestoredTrade(trade);
            }
        }

        private void AddRestoredTrade(ActiveTrade trade)
        {
            var key = GetKey(trade.StrategyName, trade.AccountId);
            _activeTrades[key] = trade;
        }
    }
}