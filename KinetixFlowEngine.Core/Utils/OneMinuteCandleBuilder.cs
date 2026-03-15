namespace KinetixFlowEngine.Core.Utils
{
    public class OneMinuteCandleBuilder
    {
        private DateTime _currentMinute = DateTime.MinValue;

        private double _high;
        private double _low;
        private double _close;

        public bool Update(double price, long tradeTimestampMs, out (double High, double Low, double Close) candle)
        {
            candle = default;

            var tradeTime = DateTimeOffset
           .FromUnixTimeMilliseconds(tradeTimestampMs)
           .UtcDateTime;

            var minute = new DateTime(
            tradeTime.Year,
            tradeTime.Month,
            tradeTime.Day,
            tradeTime.Hour,
            tradeTime.Minute,
            0,
            DateTimeKind.Utc);

            if (_currentMinute == DateTime.MinValue)
            {
                _currentMinute = minute;
                _high = price;
                _low = price;
                _close = price;
                return false;
            }

            if (minute != _currentMinute)
            {
                candle = (_high, _low, _close);

                _currentMinute = minute;
                _high = price;
                _low = price;
                _close = price;

                return true;
            }

            _high = Math.Max(_high, price);
            _low = Math.Min(_low, price);
            _close = price;

            return false;
        }
    }
}