using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Trading
{
    public class PositionManager
    {
        private readonly TradePersistence _persistence;

        public event Action<ActiveTrade>? Target1Reached;
        public event Action<ActiveTrade, decimal>? TradeClosed;

        private readonly Dictionary<string, ActiveTrade> _activeTrades;

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

        public IEnumerable<ActiveTrade> GetAllPositions()
        {
            return _activeTrades.Values;
        }

        public void TryEnterTrade(StrategySignal signal, decimal price, double atr, KinetixEngineResult r)
        {
            if (_activeTrades.ContainsKey(signal.StrategyName))
                return;

            decimal atrValue = (decimal)atr;
            decimal stopDistance = Math.Min(price * 0.01m, atrValue * 3);

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

                Closed = false
            };

            _activeTrades[signal.StrategyName] = trade;

            _persistence.Save(_activeTrades);
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
                    CloseTrade(trade.StrategyName, price, trade.Target1Hit ? "TSL" : "SL");
                }

                if (trade.Direction == SignalDirection.Short && price >= trade.StopLoss)
                {
                    CloseTrade(trade.StrategyName, price, trade.Target1Hit ? "TSL" : "SL");
                }
            }

            _persistence.Save(_activeTrades);
        }

        public void CloseTrade(string strategyName, decimal exitPrice, string reason = "SL")
        {
            if (!_activeTrades.TryGetValue(strategyName, out var trade))
                return;

            if (trade.Closed)
                return;

            trade.Closed = true;
            trade.ExitReason = reason;

            _activeTrades.Remove(strategyName);

            _persistence.Save(_activeTrades);

            TradeClosed?.Invoke(trade, exitPrice);
        }
    }
}