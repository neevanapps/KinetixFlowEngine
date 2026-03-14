using System.Collections.Concurrent;
using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowTradeBuffer
    {
        private readonly ConcurrentQueue<FlowTrade> _trades = new();

        private readonly int _maxTrades;

        // Cache last trade for O(1) access
        private volatile FlowTrade? _lastTrade;

        public FlowTradeBuffer(int maxTrades = 50000)
        {
            _maxTrades = maxTrades;
        }

        public void AddTrade(FlowTrade trade)
        {
            _trades.Enqueue(trade);

            // Update cached last trade
            _lastTrade = trade;

            while (_trades.Count > _maxTrades)
            {
                _trades.TryDequeue(out _);
            }
        }

        public IEnumerable<FlowTrade> GetSnapshot()
        {
            return _trades;
        }

        public bool TryGetLast(out FlowTrade trade)
        {
            var last = _lastTrade;

            if (last == null)
            {
                trade = default!;
                return false;
            }

            trade = last;
            return true;
        }
    }
}