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

            // Score pressure
            longScore += Math.Max(scoreZ, 0);
            shortScore += Math.Max(-scoreZ, 0);

            // Velocity ignition
            longScore += Math.Max(velocityZ, 0);
            shortScore += Math.Max(-velocityZ, 0);

            // Imbalance
            longScore += Math.Max(imbalanceZ, 0);
            shortScore += Math.Max(-imbalanceZ, 0);

            // Compression reduces probabilities
            if (compression > 0.8)
            {
                longScore *= 0.6;
                shortScore *= 0.6;
            }

            // Exhaustion penalizes continuation
            if (exhaustion > 5)
            {
                longScore *= 0.5;
                shortScore *= 0.5;
            }

            // State adjustments
            if (state.State == FlowState.Ignition)
            {
                longScore *= 1.3;
                shortScore *= 1.3;
            }

            if (state.State == FlowState.Exhaustion)
            {
                longScore *= 0.5;
                shortScore *= 0.5;
            }

            // -------------------------------------
            // FLOW-PRICE DIVERGENCE WEIGHTING
            // -------------------------------------

            if (bullishAbsorption)
            {
                longScore *= 1.4;
                shortScore *= 0.6;
            }

            if (bearishDistribution)
            {
                shortScore *= 1.4;
                longScore *= 0.6;
            }

            // Price impact control
            if (bullishControl)
            {
                longScore *= 1.35;
                shortScore *= 0.65;
            }

            if (bearishControl)
            {
                shortScore *= 1.35;
                longScore *= 0.65;
            }

            // -------------------------------------
            // PERSISTENCE BOOST
            // -------------------------------------

            if (state.Bullish && scoreTrend == FlowTrend.Bullish)
            {
                longScore *= 1.15;
            }

            if (state.Bearish && scoreTrend == FlowTrend.Bearish)
            {
                shortScore *= 1.15;
            }

            // VWAP absorption is stronger institutional signal
            if (vwapBullishAbsorption)
            {
                longScore *= 1.5;
                shortScore *= 0.5;
            }

            if (vwapBearishAbsorption)
            {
                shortScore *= 1.5;
                longScore *= 0.5;
            }

            var total = longScore + shortScore;

            if (total == 0)
                return new FlowProbabilitySnapshot();

            return new FlowProbabilitySnapshot
            {
                LongProbability = longScore / total,
                ShortProbability = shortScore / total
            };
        }
    }
}