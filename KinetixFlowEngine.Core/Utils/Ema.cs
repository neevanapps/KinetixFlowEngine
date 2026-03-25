namespace KinetixFlowEngine.Core.Utils
{
    public class Ema
    {
        private readonly double _alpha;
        private bool _initialized;
        private double _value;

        public Ema(int period)
        {
            _alpha = 2.0 / (period + 1);
        }

        public double Update(double input)
        {
            if (!_initialized)
            {
                _value = input;
                _initialized = true;
                return _value;
            }

            _value = (_alpha * input) + (1 - _alpha) * _value;
            return _value;
        }

        public double Value => _value;
    }
}