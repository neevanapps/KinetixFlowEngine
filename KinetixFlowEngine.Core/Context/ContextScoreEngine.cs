using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Context
{
    public class ContextScoreEngine
    {
        // ----------------------------------
        // BASE SCORE (no structural logic)
        // ----------------------------------
        public double AdjustScoreBase(
            double score,
            double vwapDev,
            double er,
            double oiChange)
        {
            double erMultiplier = 0.5 + er;

            double oiMultiplier = oiChange > 0 ? 1.1 : 1.0;

            double vwapMultiplier = Math.Abs(vwapDev) < 0.002 ? 1.1 : 1.0;

            var adjusted = score * erMultiplier * oiMultiplier * vwapMultiplier;

            return Math.Clamp(adjusted, -100, 100);
        }

        // ----------------------------------
        // STRUCTURAL PENALTY (IMPORTANT)
        // ----------------------------------
        public double ApplyPenalty(double score, FlowTrend priceTrend, FlowImpactSnapshot impact, bool bearishTrap, bool bullishTrap)
        {
            double penalty = 1.0;

            // Price contradiction (soft)
            if (priceTrend == FlowTrend.Bearish && score > 0)
                penalty *= 0.80;

            if (priceTrend == FlowTrend.Bullish && score < 0)
                penalty *= 0.80;

            // Impact control (moderate, not crushing)
            if (impact.BearishControl && score > 0)
                penalty *= 0.65;

            if (impact.BullishControl && score < 0)
                penalty *= 0.65;

            // Traps (light penalty)
            if (bearishTrap && score > 0)
                penalty *= 0.75;

            if (bullishTrap && score < 0)
                penalty *= 0.75;

            // Efficiency (very light)
            if (Math.Abs(impact.Efficiency) < 0.2)
                penalty *= 0.85;

            score *= penalty;

            return Math.Clamp(score, -100, 100);
        }
    }
}