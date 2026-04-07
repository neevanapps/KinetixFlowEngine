namespace KinetixFlowEngine.Core.Utils
{
    public sealed class AdaptiveEma
    {
        public decimal? Value { get; private set; }
        public decimal LastAlpha { get; private set; }
        public decimal Update(decimal price, decimal er, int fastestPeriod, int slowestPeriod)
        {
            decimal fastAlpha = 2m / (fastestPeriod + 1m);
            decimal slowAlpha = 2m / (slowestPeriod + 1m);

            decimal sc = er * (fastAlpha - slowAlpha) + slowAlpha;
            LastAlpha = sc;

            if (!Value.HasValue)
                Value = price;
            else
                Value = Value + sc * (price - Value);

            return Value.Value;
        }

        // NEW METHOD (Persistence adaptive smoothing)
        public decimal UpdateWithFactor(decimal price, decimal factor, int fastestPeriod, int slowestPeriod)
        {
            decimal fastAlpha = 2m / (fastestPeriod + 1m);
            decimal slowAlpha = 2m / (slowestPeriod + 1m);

            decimal sc = factor * (fastAlpha - slowAlpha) + slowAlpha;
            LastAlpha = sc;

            if (!Value.HasValue)
                Value = price;
            else
                Value = Value + sc * (price - Value);

            return Value.Value;
        }

        public static double AlphaToPeriod(decimal alpha)
        {
            if (alpha <= 0) return double.MaxValue;
            return (double)(2m / alpha - 1m);
        }

        public void Restore(decimal value)
        {
            Value = value;
        }
    }
}