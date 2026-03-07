namespace KinetixFlowEngine.Core.Flow.Features
{
    public static class TradeSizeBias
    {
        public static double Calculate(
            decimal buyVolume,
            decimal sellVolume,
            int buyTrades,
            int sellTrades)
        {
            if (buyTrades == 0 || sellTrades == 0)
                return 0;

            var avgBuy = buyVolume / buyTrades;
            var avgSell = sellVolume / sellTrades;

            return (double)(avgBuy - avgSell);
        }
    }
}