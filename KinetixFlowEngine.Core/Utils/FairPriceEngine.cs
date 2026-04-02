namespace KinetixFlowEngine.Core.Utils
{
    public class FairPriceEngine
    {
        private const decimal LongThreshold = -0.15m;   // prefer pullbacks
        private const decimal ShortThreshold = 0.15m;

        private const decimal MaxChase = 0.35m;         // hard block

        public bool IsFairLongEntry(decimal price, double vwap, double atr)
        {
            if (atr <= 0)
                return true;

            var dev = GetDeviationAtr(price, vwap, atr);

            // ❌ chasing — too expensive
            if (dev > MaxChase)
                return false;

            // ✅ good pullback or near fair value
            return dev <= LongThreshold;
        }

        public bool IsFairShortEntry(decimal price, double vwap, double atr)
        {
            if (atr <= 0)
                return true;

            var dev = GetDeviationAtr(price, vwap, atr);

            // ❌ chasing downside
            if (dev < -MaxChase)
                return false;

            // ✅ good pullback or near fair value
            return dev >= ShortThreshold;
        }

        public decimal GetDeviationAtr(decimal price, double vwap, double atr)
        {
            if (atr <= 0)
                return 0;

            var deviation = price - (decimal)vwap;

            return deviation / (decimal)atr;
        }

        // -------------------------------
        // NEW: Fair price levels (for logs)
        // -------------------------------
        public decimal GetFairLongPrice(double vwap, double atr)
        {
            if (atr <= 0) return (decimal)vwap;
            return (decimal)vwap + LongThreshold * (decimal)atr;
        }

        public decimal GetFairShortPrice(double vwap, double atr)
        {
            if (atr <= 0) return (decimal)vwap;
            return (decimal)vwap + ShortThreshold * (decimal)atr;
        }

        public decimal GetMaxChaseLongPrice(double vwap, double atr)
        {
            if (atr <= 0) return (decimal)vwap;
            return (decimal)vwap + MaxChase * (decimal)atr;
        }

        public decimal GetMaxChaseShortPrice(double vwap, double atr)
        {
            if (atr <= 0) return (decimal)vwap;
            return (decimal)vwap - MaxChase * (decimal)atr;
        }
    }
}