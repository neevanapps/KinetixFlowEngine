using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Utils
{
    public class FifteenMinuteCandleBuilder
    {
        private long _currentBucket = -1;
        private double _high, _low, _close;

        public bool Update(double price, long timestampMs, out (double High, double Low, double Close) candle)
        {
            candle = default;
            long bucket = timestampMs / (15 * 60000);   // 15-minute bucket

            if (_currentBucket == -1)
            {
                _currentBucket = bucket;
                _high = _low = _close = price;
                return false;
            }

            if (bucket != _currentBucket)
            {
                candle = (_high, _low, _close);
                _currentBucket = bucket;
                _high = _low = _close = price;
                return true;
            }

            _high = Math.Max(_high, price);
            _low = Math.Min(_low, price);
            _close = price;
            return false;
        }
    }
}
