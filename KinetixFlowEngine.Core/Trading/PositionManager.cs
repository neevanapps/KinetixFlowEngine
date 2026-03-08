using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Trading
{
    public class PositionManager
    {
        private readonly TradePersistence _persistence;

        private ActiveTrade? _activeTrade;

        public PositionManager(TradePersistence persistence)
        {
            _persistence = persistence;

            _activeTrade = _persistence.Load();
        }

        public ActiveTrade? ActiveTrade => _activeTrade;

        public bool HasPosition => _activeTrade != null;

        public void TryEnterTrade(StrategySignal signal, decimal price)
        {
            if (_activeTrade != null)
                return;

            var stopDistance = price * 0.002m; // 0.2% placeholder

            var trade = new ActiveTrade
            {
                StrategyName = signal.StrategyName,
                Direction = signal.Direction,
                EntryPrice = price,
                StopLoss = signal.Direction == SignalDirection.Long
                    ? price - stopDistance
                    : price + stopDistance,
                Target1 = signal.Direction == SignalDirection.Long
                    ? price + stopDistance * 2
                    : price - stopDistance * 2,
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
                if (trade.Direction == SignalDirection.Long && price >= trade.Target1)
                {
                    trade.Target1Hit = true;

                    trade.RemainingSize *= 0.5m;
                }

                if (trade.Direction == SignalDirection.Short && price <= trade.Target1)
                {
                    trade.Target1Hit = true;

                    trade.RemainingSize *= 0.5m;
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