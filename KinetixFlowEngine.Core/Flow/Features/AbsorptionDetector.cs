namespace KinetixFlowEngine.Core.Flow.Features
{
    public static class AbsorptionDetector
    {
        public static double Detect(
            decimal buyVolume,
            decimal sellVolume,
            int buyTrades,
            int sellTrades)
        {
            if (buyTrades == 0 || sellTrades == 0)
                return 0;

            var avgBuy = buyVolume / buyTrades;
            var avgSell = sellVolume / sellTrades;

            if (sellTrades > buyTrades && avgBuy > avgSell)
                return 1; // bullish absorption

            if (buyTrades > sellTrades && avgSell > avgBuy)
                return -1; // bearish absorption

            return 0;
        }
    }
}