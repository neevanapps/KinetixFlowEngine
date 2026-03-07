using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Trend
{
    public class ScoreTrendEngine
    {
        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _slow = new();

        public decimal Fast => _fast.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;

        public FlowTrend Update(decimal score, decimal er)
        {
            var fast = _fast.Update(score, er, 10 * 60, 40 * 60);
            var slow = _slow.Update(score, er, 40 * 60, 160 * 60);

            if (fast > slow)
                return FlowTrend.Bullish;

            if (fast < slow)
                return FlowTrend.Bearish;

            return FlowTrend.Neutral;
        }

        public void Restore(decimal fast, decimal slow)
        {
            _fast.Restore(fast);
            _slow.Restore(slow);
        }
    }
}