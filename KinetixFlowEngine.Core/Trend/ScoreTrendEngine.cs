using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Utils;


namespace KinetixFlowEngine.Core.Trend
{
    public class ScoreTrendEngine
    {
        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _medium = new();
        private readonly AdaptiveEma _slow = new();

        private const int minTick = 12;

        public decimal Fast => _fast.Value ?? 0;
        public decimal Medium => _medium.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;

        private readonly FlowMomentumRun _momentumRun;

        public ScoreTrendEngine(FlowMomentumRun momentumRun)
        {
            _momentumRun = momentumRun;
        }

        public FlowTrend Update(decimal score, double velocityZ)
        {
            // ←←← NO normalization anymore (raw score now, same as Probability)
            decimal factor = _momentumRun.GetFactor((double)score, velocityZ);

            var fast = _fast.UpdateWithFactor(score, factor, 5 * minTick, 15 * minTick);
            var medium = _medium.UpdateWithFactor(score, factor, 15 * minTick, 45 * minTick);
            var slow = _slow.UpdateWithFactor(score, factor, 45 * minTick, 135 * minTick);

            const decimal hysteresis = 0.5m;

            if (fast > slow && (fast - slow) > hysteresis)
                return FlowTrend.Bullish;

            if (fast < slow && (slow - fast) > hysteresis)
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