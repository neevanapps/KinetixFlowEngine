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

            var first = _prices.First();
            var last = _prices.Last();

            double change = Math.Abs(last - first);

            double volatility = 0;
            double prev = first;

            foreach (var p in _prices)
            {
                volatility += Math.Abs(p - prev);
                prev = p;
            }

            if (volatility == 0)
                return _lastEr;

            _lastEr = change / volatility;

            return _lastEr;
        }
    }
}