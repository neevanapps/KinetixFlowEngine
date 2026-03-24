using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Flow.Probability
{
    public class FlowProbabilityEngine
    {
        public FlowProbabilitySnapshot Calculate(
            double scoreZ,
            double velocityZ,
            double imbalanceZ,
            double compression,
            double exhaustion,
            FlowStateSnapshot state,
            FlowTrend scoreTrend,
            bool bullishAbsorption,
            bool bearishDistribution,
            bool vwapBullishAbsorption,
            bool vwapBearishAbsorption,
            bool bullishControl,
            bool bearishControl)
        {
            double s = Math.Clamp(scoreZ, -3.0, 3.0);
            double v = Math.Clamp(velocityZ, -3.0, 3.0);
            double i = Math.Clamp(imbalanceZ, -3.0, 3.0);

            double directional = s * 0.55 + v * 0.25 + i * 0.20;

            double context = 0;

            if (compression > 0.75)
                context += Math.Sign(v) * 0.25;               // reduced from 0.35

            double exhaustionPenalty = Math.Min(exhaustion / 15.0, 0.25);
            directional *= (1 - exhaustionPenalty);

            if (state.State == FlowState.Ignition || state.State == FlowState.TrendContinuation)
                context += Math.Sign(directional) * 0.20;     // reduced from 0.25

            if (state.State == FlowState.Exhaustion)
                directional *= 0.85;

            if (bullishAbsorption) context += 0.18;
            if (bearishDistribution) context -= 0.18;

            if (vwapBullishAbsorption) context += 0.12;
            if (vwapBearishAbsorption) context -= 0.12;

            if (bullishControl) context += 0.12;
            if (bearishControl) context -= 0.12;

            if (scoreTrend == FlowTrend.Bullish) context += 0.22;   // reduced from 0.30
            if (scoreTrend == FlowTrend.Bearish) context -= 0.22;

            // Directional stays dominant
            double finalScore = directional * 0.75 + context * 0.25;

            double prob = 1.0 / (1.0 + Math.Exp(-finalScore * 1.1));
            prob = Math.Clamp(prob, 0.10, 0.90);

            return new FlowProbabilitySnapshot
            {
                LongProbability = prob,
                ShortProbability = 1 - prob
            };
        }
    }
}