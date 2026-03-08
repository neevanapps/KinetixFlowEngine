namespace KinetixFlowEngine.Core.Utils
{
    public class OneMinuteCandleBuilder
    {
        private DateTime _currentMinute = DateTime.MinValue;

        private double _high;
        private double _low;
        private double _close;

        public bool Update(double price, out (double High, double Low, double Close) candle)
        {
            candle = default;

            var minute = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                DateTime.UtcNow.Day,
                DateTime.UtcNow.Hour,
                DateTime.UtcNow.Minute,
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