using KinetixFlowEngine.Core.Models;
using System.Collections.Concurrent;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowAggregationWindow
    {
        private readonly ConcurrentQueue<FlowTrade> _windowTrades = new();

        private readonly int _windowSeconds;

        private decimal _buyVolume;
        private decimal _sellVolume;

        private int _buyTrades;
        private int _sellTrades;

        public FlowAggregationWindow(int windowSeconds = 60)
        {
            _windowSeconds = windowSeconds;
        }

        public void AddTrade(FlowTrade trade)
        {
            _windowTrades.Enqueue(trade);

            if (!trade.IsBuyerMaker)
            {
                _buyVolume += trade.Quantity;
                _buyTrades++;
            }
            else
            {
                _sellVolume += trade.Quantity;
                _sellTrades++;
            }

            RemoveExpiredTrades(trade.Timestamp);
        }

        private void RemoveExpiredTrades(long latestTradeTimestamp)
        {
            long cutoff = latestTradeTimestamp - (_windowSeconds * 1000);
            while (_windowTrades.TryPeek(out var oldTrade))
            {
                if (oldTrade.Timestamp >= cutoff)
                    break;

                _windowTrades.TryDequeue(out var removed);

                if (!removed.IsBuyerMaker)
                {
                    _buyVolume -= removed.Quantity;
                    _buyTrades--;
                }
                else
                {
                    _sellVolume -= removed.Quantity;
                    _sellTrades--;
                }
            }
        }

        public FlowWindowSnapshot GetSnapshot()
        {
            return new FlowWindowSnapshot
            {
                BuyVolume = _buyVolume,
                SellVolume = _sellVolume,
                BuyTrades = _buyTrades,
                SellTrades = _sellTrades
            };
        }
    }
}