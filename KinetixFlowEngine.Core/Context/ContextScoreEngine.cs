using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Context
{
    public class ContextScoreEngine
    {
        // ----------------------------------
        // CONTEXT ADJUSTMENT (DIRECTIONAL, PROPORTIONAL)
        // ----------------------------------
        public double AdjustScore(
            double rawScore,
            double vwapDev,
            double er,
            double oiChange,
            FlowTrend priceTrend)
        {
            double score = rawScore;

            // ==============================
            // 1. TREND ALIGNMENT (ER + PRICE TREND)
            // ==============================
            if (priceTrend == FlowTrend.Bullish && score > 0)
                score *= (1.0 + er * 0.3);

            else if (priceTrend == FlowTrend.Bearish && score < 0)
                score *= (1.0 + er * 0.3);

            else if (priceTrend != FlowTrend.Neutral)
                score *= 0.7; // conflict

            // ==============================
            // 2. VWAP POSITIONING (DIRECTIONAL)
            // ==============================
            double vwapImpact = Math.Clamp(Math.Abs(vwapDev) * 20, 0, 3);

            if (score > 0 && vwapDev > 0)
                score += vwapImpact;

            else if (score < 0 && vwapDev < 0)
                score -= vwapImpact;

            else
                score *= 0.85; // wrong side of VWAP

            // ==============================
            // 3. OI CONFIRMATION (WEAK, NOT DOMINANT)
            // ==============================
            double oiImpact = Math.Clamp(oiChange * 0.5, -2, 2);

            if (Math.Sign(score) == Math.Sign(oiImpact))
                score += oiImpact;
            else
                score *= 0.9;

            // ==============================
            // FINAL CLAMP
            // ==============================
            return Math.Clamp(score, -20, 20);
        }

        // ----------------------------------
        // STRUCTURAL FILTER (SIMPLIFIED)
        // ----------------------------------
        public double ApplyStructureFilter(
            double score,
            FlowImpactSnapshot impact,
            bool bearishTrap,
            bool bullishTrap,
            bool highPersistence,
            bool volumeExpansion)
        {
            double result = score;

            // Impact conflict
            if ((impact.BearishControl && result > 0) ||
                (impact.BullishControl && result < 0))
                result *= 0.75;

            // Trap detection
            if ((bearishTrap && result > 0) ||
                (bullishTrap && result < 0))
                result *= 0.7;

            // Low efficiency = weak move
            if (Math.Abs(impact.Efficiency) < 0.2)
                result *= 0.85;

            // Strong conviction override
            if (highPersistence && volumeExpansion)
                result *= 1.1;

            return Math.Clamp(result, -20, 20);
        }
    }
}