using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Context
{
    public class FundingRateEngine
    {
        private readonly Ema _ema = new Ema(10);
        private double _lastRate = 0;

        public double CurrentRate { get; private set; } = 0;

        public double Update(double newRate)
        {
            CurrentRate = newRate;

            if (_lastRate == 0)
            {
                _lastRate = newRate;
                return 0;
            }

            double change = newRate - _lastRate;
            _lastRate = newRate;

            return _ema.Update(change);
        }

        public double FundingPressure => _ema.Value;
    }
}