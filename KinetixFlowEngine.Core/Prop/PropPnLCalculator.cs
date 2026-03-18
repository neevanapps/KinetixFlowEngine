using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Prop
{
    public static class PropPnLCalculator
    {
        public static decimal CalculateUnrealizedPnL(IEnumerable<ActiveTrade> trades, decimal currentPrice)
        {
            decimal total = 0;

            foreach (var trade in trades)
            {
                if (trade.Closed)
                    continue;

                var entry = trade.EntryPrice;
                var size = trade.RemainingSize;

                decimal pnl;

                if (trade.Direction == SignalDirection.Long)
                    pnl = (currentPrice - entry) * size;
                else
                    pnl = (entry - currentPrice) * size;

                total += pnl;
            }

            return total;
        }
    }
}