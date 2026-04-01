using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Trend
{
    public class ProbabilityTrendEngine
    {
        private const int _minTicks = 12;   // 1 minute

        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _medium = new();
        private readonly AdaptiveEma _slow = new();
        public decimal Fast => _fast.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;
        public decimal Medium => _medium.Value ?? 0;

        private readonly FlowMomentumRun _momentumRun;

        public ProbabilityTrendEngine(FlowMomentumRun momentumRun)
        {
            _momentumRun = momentumRun;
        }

        public FlowTrend Update(decimal prob, double velocityZ, bool highPersistence, bool volumeExpansion)
        {
            decimal factor = _momentumRun.GetFactor((double)prob, velocityZ);

            decimal slowFactor = factor;

            if (highPersistence && volumeExpansion)
                slowFactor = Math.Clamp(factor * 0.85m, 0.6m, 1.2m);

            var fast = _fast.UpdateWithFactor(prob, factor, 10 * _minTicks, 15 * _minTicks);
            var medium = _medium.UpdateWithFactor(prob, factor, 15 * _minTicks, 30 * _minTicks);
            var slow = _slow.UpdateWithFactor(prob, slowFactor, 30 * _minTicks, 60 * _minTicks);

            if (fast > medium && medium > slow && (fast - slow) > 0.08m)
                return FlowTrend.Bullish;

            if (fast < medium && medium < slow && (slow - fast) > 0.08m)
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