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
            //------------------------------------------------
            // Clamp inputs
            //------------------------------------------------
            double s = Math.Clamp(scoreZ, -2.5, 2.5);
            double v = Math.Clamp(velocityZ, -2.5, 2.5);
            double i = Math.Clamp(imbalanceZ, -2.5, 2.5);

            //------------------------------------------------
            // 1. DIRECTIONAL CORE (single signal, NOT split)
            //------------------------------------------------
            double directional =
                  s * 0.55
                + v * 0.25
                + i * 0.20;

            //------------------------------------------------
            // 2. CONTEXT (light influence only)
            //------------------------------------------------
            double context = 0;

            // compression breakout
            if (compression > 0.8)
                context += Math.Sign(v) * 0.20;

            // exhaustion (reduced impact)
            double exhaustionPenalty = Math.Min(exhaustion / 12.0, 0.4);
            directional *= (1 - exhaustionPenalty);

            // flow state
            if (state.State == FlowState.Ignition || state.State == FlowState.TrendContinuation)
                context += Math.Sign(directional) * 0.15;

            if (state.State == FlowState.Exhaustion)
                directional *= 0.75;

            //------------------------------------------------
            // 3. SIGNALS (additive only, no multiplication)
            //------------------------------------------------
            if (bullishAbsorption) context += 0.15;
            if (bearishDistribution) context -= 0.15;

            if (vwapBullishAbsorption) context += 0.10;
            if (vwapBearishAbsorption) context -= 0.10;

            if (bullishControl) context += 0.08;
            if (bearishControl) context -= 0.08;

            //------------------------------------------------
            // 4. TREND ALIGNMENT (critical)
            //------------------------------------------------
            if (scoreTrend == FlowTrend.Bullish)
                context += 0.20;

            if (scoreTrend == FlowTrend.Bearish)
                context -= 0.20;

            //------------------------------------------------
            // 5. FINAL SCORE
            //------------------------------------------------
            double finalScore = directional + context;

            //------------------------------------------------
            // 6. LOGISTIC (NOT softmax)
            //------------------------------------------------
            double prob = 1.0 / (1.0 + Math.Exp(-finalScore));

            prob = Math.Clamp(prob, 0.05, 0.95);

            return new FlowProbabilitySnapshot
            {
                LongProbability = prob,
                ShortProbability = 1 - prob
            };
        }
    }
}