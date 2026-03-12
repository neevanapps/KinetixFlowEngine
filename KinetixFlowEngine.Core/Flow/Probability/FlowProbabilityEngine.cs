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
            double longScore = 0;
            double shortScore = 0;

            // ------------------------------------------------
            // 1. Clamp Z-scores to prevent outliers dominating
            // ------------------------------------------------

            double s = Math.Clamp(scoreZ, -2, 2);
            double v = Math.Clamp(velocityZ, -2, 2);
            double i = Math.Clamp(imbalanceZ, -2, 2);

            longScore += Math.Max(s, 0);
            shortScore += Math.Max(-s, 0);

            longScore += Math.Max(v, 0);
            shortScore += Math.Max(-v, 0);

            longScore += Math.Max(i, 0);
            shortScore += Math.Max(-i, 0);

            // ------------------------------------------------
            // 2. Compression penalty (lighter reduction)
            // ------------------------------------------------

            if (compression > 0.8)
            {
                longScore *= 0.85;
                shortScore *= 0.85;
            }

            // ------------------------------------------------
            // 3. Exhaustion smooth decay
            // ------------------------------------------------

            double penalty = Math.Min(exhaustion / 10.0, 0.5);

            longScore *= (1 - penalty);
            shortScore *= (1 - penalty);

            // ------------------------------------------------
            // 4. Flow State adjustments
            // ------------------------------------------------

            if (state.State == FlowState.Ignition)
            {
                longScore += 0.3;
                shortScore += 0.3;
            }

            if (state.State == FlowState.Exhaustion)
            {
                longScore *= 0.7;
                shortScore *= 0.7;
            }

            // ------------------------------------------------
            // 5. Flow / Price divergence signals
            // ------------------------------------------------

            if (bullishAbsorption)
            {
                longScore += 0.4;
                shortScore -= 0.2;
            }

            if (bearishDistribution)
            {
                shortScore += 0.4;
                longScore -= 0.2;
            }

            // ------------------------------------------------
            // 6. Price impact control
            // ------------------------------------------------

            if (bullishControl)
            {
                longScore += 0.3;
                shortScore -= 0.15;
            }

            if (bearishControl)
            {
                shortScore += 0.3;
                longScore -= 0.15;
            }

            // ------------------------------------------------
            // 7. Trend persistence boost
            // ------------------------------------------------

            if (state.Bullish && scoreTrend == FlowTrend.Bullish)
            {
                longScore += 0.25;
            }

            if (state.Bearish && scoreTrend == FlowTrend.Bearish)
            {
                shortScore += 0.25;
            }

            // ------------------------------------------------
            // 8. VWAP absorption (strong institutional signal)
            // ------------------------------------------------

            if (vwapBullishAbsorption)
            {
                longScore += 0.5;
                shortScore -= 0.25;
            }

            if (vwapBearishAbsorption)
            {
                shortScore += 0.5;
                longScore -= 0.25;
            }

            // ------------------------------------------------
            // 9. Prevent negative scores
            // ------------------------------------------------

            if (longScore < 0) longScore = 0;
            if (shortScore < 0) shortScore = 0;

            // ------------------------------------------------
            // 10. Convert to probabilities
            // ------------------------------------------------

            double total = longScore + shortScore;

            if (total <= 0)
            {
                return new FlowProbabilitySnapshot
                {
                    LongProbability = 0.5,
                    ShortProbability = 0.5
                };
            }

            return new FlowProbabilitySnapshot
            {
                LongProbability = longScore / total,
                ShortProbability = shortScore / total
            };
        }
    }
}