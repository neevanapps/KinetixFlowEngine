using System.Collections.Concurrent;
using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowTradeBuffer
    {
        private readonly ConcurrentQueue<FlowTrade> _trades = new();

        private readonly int _maxTrades;

        public FlowTradeBuffer(int maxTrades = 50000)
        {
            _maxTrades = maxTrades;
        }

        public void AddTrade(FlowTrade trade)
        {
            _trades.Enqueue(trade);

            while (_trades.Count > _maxTrades)
            {
                _trades.TryDequeue(out _);
            }
        }

        public FlowTrade[] GetSnapshot()
        {
            return _trades.ToArray();
        }
    }
}