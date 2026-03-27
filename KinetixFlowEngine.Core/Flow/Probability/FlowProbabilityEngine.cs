using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Flow.Probability
{
    public class FlowProbabilityEngine
    {
        public FlowProbabilitySnapshot Calculate(
            double score,                // finalScore (NOT normalized)
            double velocity,
            double persistence,
            decimal fastEma,
            decimal mediumEma,
            decimal slowEma,
            bool volumeExpansion,
            double exhaustion,
            bool bullishControl,
            bool bearishControl)
        {
            double prob = 0.5;

            // Direction dead zone
            double direction = Math.Abs(score) < 1.0 ? 0 : Math.Sign(score);

            // ============================
            // 1. SCORE STRENGTH
            // ============================
            double scoreFactor = Math.Clamp(Math.Abs(score) / 10.0, 0, 1);
            prob += direction * scoreFactor * 0.20;

            // ============================
            // 2. VELOCITY
            // ============================
            if (direction != 0 && Math.Sign(velocity) == direction)
                prob += 0.10;
            else if (direction != 0)
                prob -= 0.10;

            // ============================
            // 3. PERSISTENCE
            // ============================
            double persistenceFactor = Math.Clamp(persistence / 5.0, 0, 1);
            prob += persistenceFactor * 0.15 * direction;

            // ============================
            // 4. EMA STRUCTURE
            // ============================
            bool aligned =
                (fastEma > mediumEma && mediumEma > slowEma && direction > 0) ||
                (fastEma < mediumEma && mediumEma < slowEma && direction < 0);

            if (aligned)
                prob += 0.15 * direction;
            else if (direction != 0)
                prob -= 0.05 * direction;

            // ============================
            // 5. VOLUME
            // ============================
            if (volumeExpansion && direction != 0)
                prob += 0.08 * direction;

            // ============================
            // 6. CONTROL
            // ============================
            if (bullishControl && direction > 0)
                prob += 0.08;

            if (bearishControl && direction < 0)
                prob -= 0.08;

            // ============================
            // EXHAUSTION
            // ============================
            double exhaustionClamped = Math.Clamp(exhaustion, 0, 15);
            double factor = 1.0 - (exhaustionClamped / 15.0) * 0.3;

            double distanceFromNeutral = prob - 0.5;
            prob = 0.5 + (distanceFromNeutral * factor);

            // ============================
            // FINAL CLAMP
            // ============================
            prob = Math.Clamp(prob, 0.05, 0.95);

            return new FlowProbabilitySnapshot
            {
                LongProbability = prob,
                ShortProbability = 1 - prob
            };
        }
    }
}