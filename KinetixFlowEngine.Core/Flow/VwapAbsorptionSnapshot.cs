using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Flow
{
    public class VwapAbsorptionSnapshot
    {
        public bool BullishAbsorption { get; set; }
        public bool BearishAbsorption { get; set; }
        public double Strength { get; set; }
    }

    public class VwapAbsorptionEngine
    {
        public VwapAbsorptionSnapshot Detect(
            double price,
            double vwap,
            double score,
            FlowTrend priceTrend)
        {
            bool bullish = false;
            bool bearish = false;
            double strength = 0;

            double deviation = (price - vwap) / vwap;

            // Bullish absorption
            if (priceTrend == FlowTrend.Bullish && price > vwap && score < 0)
            {
                bullish = true;
                strength = Math.Abs(score);
            }

            // Bearish absorption
            if (priceTrend == FlowTrend.Bearish && price < vwap && score > 0)
            {
                bearish = true;
                strength = Math.Abs(score);
            }

            if (Math.Abs(deviation) < 0.002)
                strength *= 1.2;

            return new VwapAbsorptionSnapshot
            {
                BullishAbsorption = bullish,
                BearishAbsorption = bearish,
                Strength = strength
            };
        }
    }
}