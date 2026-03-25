using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Trend
{
    public class ScoreTrendEngine
    {
        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _medium = new();
        private readonly AdaptiveEma _slow = new();
        public decimal Fast => _fast.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;
        public decimal Medium => _medium.Value ?? 0;

        private readonly FlowMomentumRun _momentumRun;
        private const decimal Hysteresis = 0.6m;

        private const double HighPersistenceThreshold = 4.0;
        private const decimal SlowBoostWhenStrong = 1.35m;   // extremely conservative

        public ScoreTrendEngine(FlowMomentumRun momentumRun)
        {
            _momentumRun = momentumRun;
        }

        public FlowTrend Update(decimal score, double velocityZ, bool highPersistence, bool volumeExpansion)
        {
            decimal factor = _momentumRun.GetFactor((double)score, velocityZ);

            // Gentle Slow-only boost ONLY on strong confluence
            decimal slowFactor = factor;
            if (highPersistence && volumeExpansion)
                slowFactor = Math.Clamp(factor * SlowBoostWhenStrong, 0.8m, 2.5m);

            var fast = _fast.UpdateWithFactor(score, factor, 12, 36);     // unchanged
            var medium = _medium.UpdateWithFactor(score, factor, 36, 108);  // unchanged
            var slow = _slow.UpdateWithFactor(score, slowFactor, 120, 360);

            if (fast > slow && (fast - slow) > Hysteresis) return FlowTrend.Bullish;
            if (fast < slow && (slow - fast) > Hysteresis) return FlowTrend.Bearish;
            return FlowTrend.Neutral;
        }

        public void Restore(decimal fast, decimal slow, decimal medium)
        {
            _fast.Restore(fast);
            _slow.Restore(slow);
            _medium.Restore(medium);
        }
    }
}