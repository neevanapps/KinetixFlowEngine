using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Strategy;
using System.Diagnostics;

namespace KinetixFlowEngine.Core.Trading
{
    public class PositionManager
    {
        private readonly TradePersistence _persistence;
        public event Action<ActiveTrade>? Target1Reached;

        private readonly Dictionary<string, ActiveTrade> _activeTrades;

        public event Action<ActiveTrade, decimal>? TradeClosed;

        public PositionManager(TradePersistence persistence)
        {
            _persistence = persistence;
            _activeTrades = _persistence.Load();
        }

        public bool HasPosition(string strategy)
        {
            return _activeTrades.ContainsKey(strategy);
        }

        public ActiveTrade? GetPosition(string strategy)
        {
            _activeTrades.TryGetValue(strategy, out var trade);
            return trade;
        }

        public void TryEnterTrade(StrategySignal signal, decimal price, double atr, KinetixEngineResult r)
        {
            if (_activeTrades.ContainsKey(signal.StrategyName))
                return;

            decimal atrValue = (decimal)atr;
            var stopDistance = Math.Min(price * 0.01m, atrValue * 3);

            var trade = new ActiveTrade
            {
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
                InitialSize = 1,
                RemainingSize = 1,
                EntryTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                NotifyThroughTelegram = signal.NotifyThroughTelegram,
                MaxPrice = price,
                MinPrice = price,
                EntryScoreZ = r.ScoreZ,
                EntryVelocityZ = r.VelocityZ,
                EntryImbalanceZ = r.ImbalanceZ,
                EntryCompressionZ = r.CompressionZ,
                EntryATR = r.ATR,
                EntryER = r.ER,
                EntryFlowState = r.FlowState.State.ToString(),
            };

            _activeTrades[signal.StrategyName] = trade;

            _persistence.Save(_activeTrades);
        }

        public void Update(decimal price)
        {
            foreach (var trade in _activeTrades.Values.ToList())
            {
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

                        Target1Reached?.Invoke(trade);
                    }
                }

                if (trade.Direction == SignalDirection.Long && price <= trade.StopLoss)
                {
                    CloseTrade(trade.StrategyName, price);
                }

                if (trade.Direction == SignalDirection.Short && price >= trade.StopLoss)
                {
                    CloseTrade(trade.StrategyName, price);
                }
            }

            _persistence.Save(_activeTrades);
        }

        public IEnumerable<ActiveTrade> GetAllPositions()
        {
            return _activeTrades.Values;
        }

        public void CloseTrade(string strategy, decimal exitPrice)
        {
            if (!_activeTrades.TryGetValue(strategy, out var trade))
                return;

            TradeClosed?.Invoke(trade, exitPrice);

            _activeTrades.Remove(strategy);

            _persistence.Save(_activeTrades);
        }
    }
}