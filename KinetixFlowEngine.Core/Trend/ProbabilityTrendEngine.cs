using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Trend
{
    public class ProbabilityTrendEngine
    {
        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _medium = new();
        private readonly AdaptiveEma _slow = new();
        private const int minTick = 6;
        public decimal Fast => _fast.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;
        public decimal Medium => _medium.Value ?? 0;

        public FlowTrend Update(decimal prob, decimal er)
        {
            var fast = _fast.Update(prob, er, 6 * minTick, 20 * minTick);
            var medium = _medium.Update(prob, er, 20 * minTick, 60 * minTick);
            var slow = _slow.Update(prob, er, 60 * minTick, 180 * minTick);

            if (fast > medium && medium > slow)
                return FlowTrend.Bullish;

            if (fast < medium && medium < slow)
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