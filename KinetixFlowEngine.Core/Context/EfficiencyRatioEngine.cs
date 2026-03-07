namespace KinetixFlowEngine.Core.Context
{
    public class EfficiencyRatioEngine
    {
        private readonly Queue<double> _prices = new();
        private const int Period = 20;

        public double Update(double price)
        {
            _prices.Enqueue(price);

            if (_prices.Count > Period)
                _prices.Dequeue();

            if (_prices.Count < Period)
                return 0.5;

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
                return 0;

            return change / volatility;
        }
    }
}