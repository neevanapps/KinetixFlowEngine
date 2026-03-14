using System.Collections.Generic;

namespace KinetixFlowEngine.Core.Context
{
    public class VwapEngine
    {
        private class Trade
        {
            public decimal Price;
            public decimal Volume;
            public long Timestamp;
        }

        private readonly Queue<Trade> _trades = new();

        // 15 minutes
        private const int WindowSeconds = 900;

        private decimal _cumVolume;
        private decimal _cumPriceVolume;

        public decimal Update(decimal price, decimal volume, long now)
        {
            var trade = new Trade
            {
                Price = price,
                Volume = volume,
                Timestamp = now
            };

            _trades.Enqueue(trade);

            _cumVolume += volume;
            _cumPriceVolume += price * volume;

            RemoveOldTrades(now);

            if (_cumVolume == 0)
                return price;

            return _cumPriceVolume / _cumVolume;
        }

        private void RemoveOldTrades(long now)
        {
            long cutoff = now - (WindowSeconds * 1000);

            while (_trades.Count > 0)
            {
                var t = _trades.Peek();

                if (t.Timestamp >= cutoff)
                    break;

                _trades.Dequeue();

                _cumVolume -= t.Volume;
                _cumPriceVolume -= t.Price * t.Volume;
            }
        }

        public double Deviation(decimal price, decimal vwap)
        {
            if (vwap == 0)
                return 0;

            return (double)((price - vwap) / vwap);
        }
    }
}