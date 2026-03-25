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
            // Slightly less punitive ER multiplier (was too aggressive)
            double erMultiplier = 0.75 + (er * 0.4);   // minimum ~0.75 even when ER5 is low

            double oiMultiplier = oiChange > 0 ? 1.1 : 1.0;

            double vwapMultiplier = Math.Abs(vwapDev) < 0.002 ? 1.1 : 1.0;

            var adjusted = score * erMultiplier * oiMultiplier * vwapMultiplier;

            return Math.Clamp(adjusted, -100, 100);
        }

        // ----------------------------------
        // STRUCTURAL PENALTY (CONVICTION-AWARE)
        // ----------------------------------
        public double ApplyPenalty(double score, FlowTrend priceTrend, FlowImpactSnapshot impact, bool bearishTrap, bool bullishTrap,
                                   bool highPersistence, bool volumeExpansion)
        {
            double penalty = 1.0;

            // Very soft price contradiction
            if (priceTrend == FlowTrend.Bearish && score > 0)
                penalty *= 0.94;

            if (priceTrend == FlowTrend.Bullish && score < 0)
                penalty *= 0.94;

            // Soft impact control
            if (impact.BearishControl && score > 0)
                penalty *= 0.85;

            if (impact.BullishControl && score < 0)
                penalty *= 0.85;

            // Very light traps
            if (bearishTrap && score > 0)
                penalty *= 0.92;

            if (bullishTrap && score < 0)
                penalty *= 0.92;

            // Light efficiency
            if (Math.Abs(impact.Efficiency) < 0.2)
                penalty *= 0.95;

            // CONVICTION-AWARE SAFETY FLOOR (the real fix)
            if (highPersistence && volumeExpansion && score > 8)
                penalty = Math.Max(penalty, 0.88);   // strong flow gets protected

            score *= penalty;

            return Math.Clamp(score, -100, 100);
        }
    }
}