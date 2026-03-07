namespace KinetixFlowEngine.Core.Utils
{
    public sealed class AdaptiveEma
    {
        public decimal? Value { get; private set; }

        public decimal Update(decimal price, decimal er, int fastestPeriod, int slowestPeriod)
        {
            decimal fastAlpha = 2m / (fastestPeriod + 1m);
            decimal slowAlpha = 2m / (slowestPeriod + 1m);

            decimal sc = er * (fastAlpha - slowAlpha) + slowAlpha;

            if (!Value.HasValue)
                Value = price;
            else
                Value = Value + sc * (price - Value);

            return Value.Value;
        }

        public void Restore(decimal value)
        {
            Value = value;
        }
    }
}