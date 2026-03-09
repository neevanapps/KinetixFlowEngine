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
        public decimal Slow => _slow.Value ?? 0;
        public decimal Medium => _medium.Value ?? 0;

        public FlowTrend Update(decimal score, decimal er)
        {
            var fast = _fast.Update(score, er, 8 * minTick, 30 * minTick);
            var medium = _medium.Update(score, er, 15 * minTick, 60 * minTick);
            var slow = _slow.Update(score, er, 50 * minTick, 200 * minTick);

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