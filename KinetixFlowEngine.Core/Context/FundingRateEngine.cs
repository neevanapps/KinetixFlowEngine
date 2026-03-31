using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Context
{
    public class FundingRateEngine
    {
        private readonly Ema _ema = new Ema(10);

        public double CurrentRate { get; private set; } = 0;

        public double Update(double newRate)
        {
            CurrentRate = newRate;

            double scaled = newRate * 10000; // 🔴 critical scaling
                                             // ignore micro-noise
            if (Math.Abs(scaled) < 0.05)
                scaled = 0;

            return _ema.Update(scaled);
        }

        public double FundingPressure => _ema.Value;
    }
}