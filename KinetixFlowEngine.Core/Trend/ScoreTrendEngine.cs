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

        public FlowTrend Update(decimal score, decimal er)
        {
            var fast = _fast.Update(score, er, 5 * 12, 20 * 12);
            var medium = _medium.Update(score, er, 20 * 12, 80 * 12);
            var slow = _slow.Update(score, er, 40 * 12, 160 * 12);

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