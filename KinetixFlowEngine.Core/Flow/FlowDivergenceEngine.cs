using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowDivergenceSnapshot
    {
        public bool BullishAbsorption { get; set; }
        public bool BearishDistribution { get; set; }
        public double Strength { get; set; }
    }

    public class FlowDivergenceEngine
    {
        public FlowDivergenceSnapshot Detect(
            FlowTrend priceTrend,
            FlowTrend scoreTrend,
            double scoreZ,
            double vwapDeviation)
        {
            bool bullishAbsorption = false;
            bool bearishDistribution = false;
            double strength = 0;

            if (priceTrend == FlowTrend.Bullish && scoreTrend == FlowTrend.Bearish)
            {
                bullishAbsorption = true;
                strength = Math.Abs(scoreZ);
            }

            if (priceTrend == FlowTrend.Bearish && scoreTrend == FlowTrend.Bullish)
            {
                bearishDistribution = true;
                strength = Math.Abs(scoreZ);
            }

            if (Math.Abs(vwapDeviation) < 0.002)
                strength *= 1.2;

            return new FlowDivergenceSnapshot
            {
                BullishAbsorption = bullishAbsorption,
                BearishDistribution = bearishDistribution,
                Strength = strength
            };
        }
    }
}