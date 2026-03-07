using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowAggregationWindow
    {
        private readonly FlowTradeBuffer _buffer;
        private readonly int _windowSeconds;

        public FlowAggregationWindow(
            FlowTradeBuffer buffer,
            int windowSeconds = 40)
        {
            _buffer = buffer;
            _windowSeconds = windowSeconds;
        }

        public FlowWindowSnapshot GetSnapshot()
        {
            var trades = _buffer.GetSnapshot();

            long cutoff = DateTimeOffset.UtcNow
                .AddSeconds(-_windowSeconds)
                .ToUnixTimeMilliseconds();

            decimal buyVolume = 0;
            decimal sellVolume = 0;

            int buyTrades = 0;
            int sellTrades = 0;

            foreach (var trade in trades)
            {
                if (trade.Timestamp < cutoff)
                    continue;

                if (!trade.IsBuyerMaker)
                {
                    buyVolume += trade.Quantity;
                    buyTrades++;
                }
                else
                {
                    sellVolume += trade.Quantity;
                    sellTrades++;
                }
            }

            return new FlowWindowSnapshot
            {
                BuyVolume = buyVolume,
                SellVolume = sellVolume,
                BuyTrades = buyTrades,
                SellTrades = sellTrades
            };
        }
    }
}