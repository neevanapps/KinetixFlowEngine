using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Trading
{
    public class PositionManager
    {
        private readonly TradePersistence _persistence;
        public event Action<ActiveTrade>? Target1Reached;
        private ActiveTrade? _activeTrade;

        public PositionManager(TradePersistence persistence)
        {
            _persistence = persistence;

            _activeTrade = _persistence.Load();
        }

        public ActiveTrade? ActiveTrade => _activeTrade;

        public bool HasPosition => _activeTrade != null;

        public void TryEnterTrade(StrategySignal signal, decimal price, double atr)
        {
            if (_activeTrade != null)
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
                NotifyThroughTelegram = signal.NotifyThroughTelegram
            };

            _activeTrade = trade;

            _persistence.Save(trade);
        }

        public void Update(decimal price)
        {
            if (_activeTrade == null)
                return;

            var trade = _activeTrade;

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
                CloseTrade();
            }

            if (trade.Direction == SignalDirection.Short && price >= trade.StopLoss)
            {
                CloseTrade();
            }

            _persistence.Save(trade);
        }

        public void CloseTrade()
        {
            _activeTrade = null;

            _persistence.Clear();
        }
    }
}