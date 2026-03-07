using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Trend
{
    public class PriceTrendEngine
    {
        private readonly AdaptiveEma _fast = new();
        private readonly AdaptiveEma _slow = new();

        public decimal Fast => _fast.Value ?? 0;
        public decimal Slow => _slow.Value ?? 0;

        public FlowTrend Update(decimal price, decimal er)
        {
            var fast = _fast.Update(price, er, 5, 20);
            var slow = _slow.Update(price, er, 25, 90);

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