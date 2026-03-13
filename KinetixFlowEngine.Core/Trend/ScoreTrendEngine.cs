using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Trend
{
    public class ScoreTrendEngine
    {
        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _medium = new();
        private readonly AdaptiveEma _slow = new();

        private const int minTick = 6;

        public decimal Fast => _fast.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;
        public decimal Medium => _medium.Value ?? 0;

        public FlowTrend Update(decimal score, int persistence)
        {
            decimal factor = PersistenceFactor(persistence);

            var fast = _fast.UpdateWithFactor(score, factor, 6 * minTick, 20 * minTick);
            var medium = _medium.UpdateWithFactor(score, factor, 20 * minTick, 60 * minTick);
            var slow = _slow.UpdateWithFactor(score, factor, 60 * minTick, 180 * minTick);

            if (fast > medium && medium > slow)
                return FlowTrend.Bullish;

            if (fast < medium && medium < slow)
                return FlowTrend.Bearish;

            return FlowTrend.Neutral;
        }

        private decimal PersistenceFactor(int persistence)
        {
            persistence = Math.Abs(persistence);

            if (persistence <= 2)
                return 0.2m;

            if (persistence <= 5)
                return 0.5m;

            return 0.9m;
        }

        public void Restore(decimal fast, decimal slow, decimal medium)
        {
            _fast.Restore(fast);
            _slow.Restore(slow);
            _medium.Restore(medium);
        }
    }
}