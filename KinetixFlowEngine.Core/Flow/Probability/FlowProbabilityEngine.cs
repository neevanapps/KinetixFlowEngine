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

            //------------------------------------------------
            // Clamp Z-scores
            //------------------------------------------------

            double s = Math.Clamp(scoreZ, -2.5, 2.5);
            double v = Math.Clamp(velocityZ, -2.5, 2.5);
            double i = Math.Clamp(imbalanceZ, -2.5, 2.5);

            //------------------------------------------------
            // Core flow signals (dominant)
            //------------------------------------------------

            longScore += Math.Max(s, 0) * 1.6;
            shortScore += Math.Max(-s, 0) * 1.6;

            longScore += Math.Max(i, 0) * 1.3;
            shortScore += Math.Max(-i, 0) * 1.3;

            longScore += Math.Max(v, 0) * 1.1;
            shortScore += Math.Max(-v, 0) * 1.1;

            //------------------------------------------------
            // Compression breakout setup
            //------------------------------------------------

            if (compression > 0.8)
            {
                if (Math.Abs(velocityZ) > 1.0)
                {
                    if (velocityZ > 0)
                        longScore += 0.35;

                    if (velocityZ < 0)
                        shortScore += 0.35;
                }
            }

            //------------------------------------------------
            // Directional exhaustion penalty
            //------------------------------------------------

            double exhaustionPenalty = Math.Min(exhaustion / 12.0, 0.55);

            if (scoreTrend == FlowTrend.Bullish)
            {
                longScore *= (1 - exhaustionPenalty * 1.4);
                shortScore *= (1 - exhaustionPenalty * 0.6);
            }
            else if (scoreTrend == FlowTrend.Bearish)
            {
                shortScore *= (1 - exhaustionPenalty * 1.4);
                longScore *= (1 - exhaustionPenalty * 0.6);
            }
            else
            {
                longScore *= (1 - exhaustionPenalty);
                shortScore *= (1 - exhaustionPenalty);
            }

            //------------------------------------------------
            // Directional flow state boost
            //------------------------------------------------

            switch (state.State)
            {
                case FlowState.Ignition:
                case FlowState.TrendContinuation:

                    if (scoreTrend == FlowTrend.Bullish || state.Bullish)
                        longScore += 0.40;
                    else if (scoreTrend == FlowTrend.Bearish || state.Bearish)
                        shortScore += 0.40;

                    break;

                case FlowState.Exhaustion:
                    longScore *= 0.7;
                    shortScore *= 0.7;
                    break;
            }

            //------------------------------------------------
            // Trend alignment boost
            //------------------------------------------------

            if (state.Bullish && scoreTrend == FlowTrend.Bullish)
                longScore += 0.35;

            if (state.Bearish && scoreTrend == FlowTrend.Bearish)
                shortScore += 0.35;

            //------------------------------------------------
            // Absorption signals
            //------------------------------------------------

            if (bullishAbsorption)
            {
                longScore += 0.40;
                shortScore -= 0.2;
            }

            if (bearishDistribution)
            {
                shortScore += 0.40;
                longScore -= 0.2;
            }

            //------------------------------------------------
            // Multiplicative VWAP absorption
            //------------------------------------------------

            if (vwapBullishAbsorption)
                longScore *= 1.35;

            if (vwapBearishAbsorption)
                shortScore *= 1.35;

            //------------------------------------------------
            // Price control
            //------------------------------------------------

            if (bullishControl)
                longScore *= 1.20;

            if (bearishControl)
                shortScore *= 1.20;

            //------------------------------------------------
            // Prevent negatives
            //------------------------------------------------

            if (longScore < 0) longScore = 0;
            if (shortScore < 0) shortScore = 0;

            //------------------------------------------------
            // Softmax probability normalization
            //------------------------------------------------

            double expLong = Math.Exp(longScore);
            double expShort = Math.Exp(shortScore);

            double sum = expLong + expShort;

            if (sum <= 0)
            {
                return new FlowProbabilitySnapshot
                {
                    LongProbability = 0.5,
                    ShortProbability = 0.5
                };
            }

            return new FlowProbabilitySnapshot
            {
                LongProbability = expLong / sum,
                ShortProbability = expShort / sum
            };
        }
    }
}