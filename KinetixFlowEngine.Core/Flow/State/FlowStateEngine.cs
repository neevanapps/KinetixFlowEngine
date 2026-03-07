using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Flow.State
{
    public class FlowStateEngine
    {
        public FlowStateSnapshot Detect(
            double scoreZ,
            double velocityZ,
            double imbalanceZ,
            double compression,
            double exhaustion,
            double persistence,
            FlowTrend scoreTrend)
        {
            var state = FlowState.Neutral;

            bool bullish = false;
            bool bearish = false;

            // ------------------------------------------------
            // Compression
            // ------------------------------------------------
            if (compression > 0.8)
            {
                state = FlowState.Compression;
            }

            // ------------------------------------------------
            // Exhaustion
            // ------------------------------------------------
            else if (exhaustion > 5)
            {
                state = FlowState.Exhaustion;
            }

            // ------------------------------------------------
            // Ignition
            // ------------------------------------------------
            else if (Math.Abs(velocityZ) > 2 && Math.Abs(scoreZ) > 1.5)
            {
                state = FlowState.Ignition;
            }

            // ------------------------------------------------
            // Trend continuation
            // ------------------------------------------------
            else if (Math.Abs(scoreZ) > 1.5 && Math.Abs(imbalanceZ) > 1)
            {
                state = FlowState.TrendContinuation;
            }

            // ------------------------------------------------
            // Accumulation / Distribution
            // ------------------------------------------------
            else if (persistence > 5 && Math.Abs(imbalanceZ) < 0.5)
            {
                state = FlowState.Accumulation;
            }

            else if (persistence < -5 && Math.Abs(imbalanceZ) < 0.5)
            {
                state = FlowState.Distribution;
            }

            // ------------------------------------------------
            // Direction
            // ------------------------------------------------
            if (scoreTrend == FlowTrend.Bullish)
                bullish = true;

            if (scoreTrend == FlowTrend.Bearish)
                bearish = true;

            return new FlowStateSnapshot
            {
                State = state,
                Bullish = bullish,
                Bearish = bearish
            };
        }
    }
}