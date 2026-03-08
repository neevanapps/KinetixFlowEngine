using System.Collections.Generic;
using System.Linq;

namespace KinetixFlowEngine.Core.Context
{
    public class EfficiencyRatioEngine
    {
        private readonly Queue<double> _prices = new();

        // 5 minute ER window
        private const int Period = 60; // 60 samples * 5s = 300 seconds

        private double _lastEr = 0.5;

        public double Update(double price)
        {
            _prices.Enqueue(price);

            if (_prices.Count > Period)
                _prices.Dequeue();

            if (_prices.Count < Period)
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