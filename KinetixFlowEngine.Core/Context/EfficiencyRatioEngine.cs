namespace KinetixFlowEngine.Core.Context
{
    public class EfficiencyRatioEngine
    {
        private readonly Queue<double> _prices = new();
        private readonly int _period;

        private double _lastEr = 0.5;

        public EfficiencyRatioEngine(int period)
        {
            _period = period;
        }

        public double Update(double price)
        {
            _prices.Enqueue(price);

            if (_prices.Count > _period)
                _prices.Dequeue();

            if (_prices.Count < _period)
                return _lastEr;

            var arr = _prices.ToArray();

            double change = Math.Abs(arr[^1] - arr[0]);

            double volatility = 0;
            for (int i = 1; i < arr.Length; i++)
            {
                volatility += Math.Abs(arr[i] - arr[i - 1]);
            }

            if (volatility < 1e-8)
                return _lastEr;

            _lastEr = change / volatility;

            return _lastEr;
        }
    }
}