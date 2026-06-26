using System.Collections.Concurrent;
using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowTradeBuffer
    {
        private readonly Queue<FlowTrade> _trades = new();

        private readonly int _retentionSeconds;

        private volatile FlowTrade? _lastTrade;

        public FlowTradeBuffer(int retentionSeconds = 120)
        {
            _retentionSeconds = retentionSeconds;
        }

        public void AddTrade(FlowTrade trade)
        {
            _trades.Enqueue(trade);

            _lastTrade = trade;

            RemoveExpiredTrades();
        }

        private void RemoveExpiredTrades()
        {
            long cutoff = DateTimeOffset.UtcNow
                .AddSeconds(-_retentionSeconds)
                .ToUnixTimeMilliseconds();

            while (_trades.Count > 0)
            {
                var oldTrade = _trades.Peek();

                if (oldTrade.Timestamp >= cutoff)
                    break;

                _trades.Dequeue();
            }
        }

        public FlowTrade[] GetSnapshot()
        {
            return _trades.ToArray();
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