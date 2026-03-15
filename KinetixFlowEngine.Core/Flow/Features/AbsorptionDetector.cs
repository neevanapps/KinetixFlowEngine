namespace KinetixFlowEngine.Core.Flow.Features
{
    public static class AbsorptionDetector
    {
        public static double Detect(
            decimal buyVolume,
            decimal sellVolume,
            double price,
            double previousPrice,
            double atr)
        {
            if (atr <= 0)
                return 0;

            double netFlow = (double)(buyVolume - sellVolume);
            double priceMove = price - previousPrice;

            //------------------------------------------------
            // Normalize by ATR
            //------------------------------------------------

            double normalizedMove = priceMove / atr;

            //------------------------------------------------
            // Absorption logic
            //------------------------------------------------

            const double flowThreshold = 0.5;   // BTC
            const double moveThreshold = 0.1;   // ATR fraction

            // Bullish absorption
            if (netFlow < -flowThreshold && normalizedMove >= -moveThreshold)
                return 1;

            // Bearish absorption
            if (netFlow > flowThreshold && normalizedMove <= moveThreshold)
                return -1;

            return 0;
        }
    }
}