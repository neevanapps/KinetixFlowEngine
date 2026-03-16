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

        public FlowTrend Update(decimal prob, int persistence)
        {
            decimal factor = PersistenceFactor(persistence);

            var fast = _fast.UpdateWithFactor(prob, factor, 6 * minTick, 20 * minTick);
            var medium = _medium.UpdateWithFactor(prob, factor, 20 * minTick, 60 * minTick);
            var slow = _slow.UpdateWithFactor(prob, factor, 60 * minTick, 180 * minTick);

            const decimal hysteresis = 0.02m;

            if (fast > medium && medium > slow && (fast - slow) > hysteresis)
                return FlowTrend.Bullish;

            if (fast < medium && medium < slow && (slow - fast) > hysteresis)
                return FlowTrend.Bearish;

            return FlowTrend.Neutral;
        }

        private decimal PersistenceFactor(int persistence)
        {
            persistence = Math.Abs(persistence);

            if (persistence <= 2)
                return 0.20m;

            if (persistence <= 5)
                return 0.35m;

            return 0.55m;
        }

        public void Restore(decimal fast, decimal slow, decimal medium)
        {
            _fast.Restore(fast);
            _slow.Restore(slow);
            _medium.Restore(medium);
        }
    }
}