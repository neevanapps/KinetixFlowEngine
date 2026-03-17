using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Trend
{
    public class ScoreTrendEngine
    {
        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _medium = new();
        private readonly AdaptiveEma _slow = new();

        private const int minTick = 8;

        public decimal Fast => _fast.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;
        public decimal Medium => _medium.Value ?? 0;

        private readonly FlowMomentumRun _momentumRun;
        public ScoreTrendEngine(FlowMomentumRun momentumRun)
        {
            _momentumRun = momentumRun;
        }

        public FlowTrend Update(decimal score, double velocityZ)
        {
            double normalizedScore = (double)(score / 100m + 0.5m); // map -100..100 → 0..1
            decimal factor = _momentumRun.GetFactor(normalizedScore, velocityZ);

            var fast = _fast.UpdateWithFactor(score, factor, 6 * minTick, 20 * minTick);
            var medium = _medium.UpdateWithFactor(score, factor, 20 * minTick, 60 * minTick);
            var slow = _slow.UpdateWithFactor(score, factor, 60 * minTick, 180 * minTick);

            const decimal hysteresis = 0.5m;

            if (fast > medium && medium > slow && (fast - slow) > hysteresis)
                return FlowTrend.Bullish;

            if (fast < medium && medium < slow && (slow - fast) > hysteresis)
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