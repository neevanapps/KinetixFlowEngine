using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Trend
{
    public class ProbabilityTrendEngine
    {
        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _medium = new(); // kept for future use / logging
        private readonly AdaptiveEma _slow = new();

        private const int minTick = 12;

        public decimal Fast => _fast.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;
        public decimal Medium => _medium.Value ?? 0;

        private readonly FlowMomentumRun _momentumRun;

        // ←←← Same public hysteresis as ScoreTrendEngine for easy sync
        public decimal Hysteresis { get; set; } = 0.25m;   // start here (same as Score)

        public ProbabilityTrendEngine(FlowMomentumRun momentumRun)
        {
            _momentumRun = momentumRun;
        }

        /// <summary>
        /// Synced Update — uses alpha when available (recommended)
        /// </summary>
        public FlowTrend Update(decimal prob, double velocityZ, double alpha = 1.0)
        {
            // Keep raw prob (0–100 range) — no extra normalization needed
            decimal factor = _momentumRun.GetFactor((double)prob, velocityZ);

            // Blend alpha gently (same idea as your new version but safer)
            decimal adjustedFactor = factor * (decimal)(alpha * 5);   // milder multiplier than *10
            adjustedFactor = Math.Clamp(adjustedFactor, 0.4m, 2.5m);  // prevents extreme swings

            // Periods aligned with ScoreTrendEngine (scaled for minTick=6)
            var fast = _fast.UpdateWithFactor(prob, adjustedFactor, 5 * minTick, 15 * minTick);
            var medium = _medium.UpdateWithFactor(prob, adjustedFactor, 15 * minTick, 45 * minTick);
            var slow = _slow.UpdateWithFactor(prob, adjustedFactor, 45 * minTick, 135 * minTick);

            // ←←← Exact same decision logic as ScoreTrendEngine
            if (fast > slow && (fast - slow) > Hysteresis)
                return FlowTrend.Bullish;

            if (fast < slow && (slow - fast) > Hysteresis)
                return FlowTrend.Bearish;

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