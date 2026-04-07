using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Trend
{
    public class ScoreTrendEngine
    {

        private const int _minTicks = 12;   // 1 minute
        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _medium = new();
        private readonly AdaptiveEma _slow = new();

        public decimal FastAlpha => _fast.LastAlpha;
        public decimal MediumAlpha => _medium.LastAlpha;
        public decimal SlowAlpha => _slow.LastAlpha;

        public decimal Fast => _fast.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;
        public decimal Medium => _medium.Value ?? 0;

        private readonly FlowMomentumRun _momentumRun;
        private const decimal Hysteresis = 0.45m;

        public ScoreTrendEngine(FlowMomentumRun momentumRun)
        {
            _momentumRun = momentumRun;
        }

        public static double AlphaToPeriod(decimal alpha)
        {
            if (alpha <= 0) return double.MaxValue;
            return (double)(2m / alpha - 1m);
        }

        public FlowTrend Update(decimal score, double velocityZ, double er, double atrNorm, bool highPersistence, bool volumeExpansion)
        {
            decimal erFactor = (decimal)Math.Clamp(er, 0.05, 0.95);
            decimal atrFactor = (decimal)(0.5 + atrNorm * 0.7); // 0.5 → 1.2
            decimal baseFactor = erFactor * atrFactor;
            decimal momentumFactor = _momentumRun.GetFactor((double)score, velocityZ);
            decimal factor = 0.7m * baseFactor + 0.3m * momentumFactor;
            factor = Math.Clamp(factor, 0.05m, 1.0m);

            decimal slowFactor = factor;
            if (highPersistence && volumeExpansion)
                slowFactor = Math.Clamp(factor * 1.05m, 0.9m, 1.3m);

            var fast = _fast.UpdateWithFactor(score, factor, 8 * _minTicks, 12 * _minTicks);
            var medium = _medium.UpdateWithFactor(score, factor, 12 * _minTicks, 24 * _minTicks);
            var slow = _slow.UpdateWithFactor(score, slowFactor, 25 * _minTicks, 50 * _minTicks);

            if (fast > medium && medium > slow && (fast - slow) > Hysteresis) return FlowTrend.Bullish;
            if (fast < medium && medium < slow && (slow - fast) > Hysteresis) return FlowTrend.Bearish;
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