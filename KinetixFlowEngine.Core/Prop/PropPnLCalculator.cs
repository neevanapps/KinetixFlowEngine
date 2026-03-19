using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;
using System.Diagnostics;

namespace KinetixFlowEngine.Core.Prop
{
    public static class PropPnLCalculator
    {
        public static decimal Calculate(
            decimal entry,
            decimal exit,
            decimal size,
            SignalDirection direction,
            decimal feeRate)
        {
            decimal gross = direction == SignalDirection.Long
                ? (exit - entry) * size
                : (entry - exit) * size;

            decimal fee = (entry + exit) * size * feeRate;

            return gross - fee;
        }

        public static decimal CalculatePartial(
            decimal entry,
            decimal target1,
            decimal exit,
            decimal size,
            decimal t1Percent,
            SignalDirection direction,
            decimal feeRate)
        {
            decimal t1Size = size * t1Percent;
            decimal t2Size = size - t1Size;

            decimal pnlT1 = direction == SignalDirection.Long
                ? (target1 - entry) * t1Size
                : (entry - target1) * t1Size;

            decimal pnlT2 = direction == SignalDirection.Long
                ? (exit - entry) * t2Size
                : (entry - exit) * t2Size;

            decimal gross = pnlT1 + pnlT2;

            decimal fee = (entry + exit) * size * feeRate;

            return gross - fee;
        }

        public static decimal CalculateUnrealizedPnL(IEnumerable<ActiveTrade> trades, decimal currentPrice)
        {
            decimal pnl = 0;

            foreach (var t in trades)
            {
                if (t.Closed)
                    continue;

                pnl += t.Direction == SignalDirection.Long
                    ? (currentPrice - t.EntryPrice) * t.InitialSize
                    : (t.EntryPrice - currentPrice) * t.InitialSize;
            }

            return pnl;
        }
    }
}