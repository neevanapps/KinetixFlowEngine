namespace KinetixFlowEngine.Core.Utils
{
    public class FifteenMinuteCandleBuilder
    {
        private DateTime _currentPeriod = DateTime.MinValue;
        private double _open;
        private double _high;
        private double _low;
        private double _close;

        public bool Update(double price, long timestampMs, out (double Open, double High, double Low, double Close) candle)
        {
            candle = default;

            var tradeTime = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).UtcDateTime;
            int minute = (tradeTime.Minute / 15) * 15;
            var period = new DateTime(
                tradeTime.Year, tradeTime.Month, tradeTime.Day,
                tradeTime.Hour, minute, 0, DateTimeKind.Utc);

            if (_currentPeriod == DateTime.MinValue)
            {
                _currentPeriod = period;
                _open = price;
                _high = price;
                _low = price;
                _close = price;
                return false;
            }

            if (period != _currentPeriod)
            {
                // Return the completed candle
                candle = (_open, _high, _low, _close);

                // Start new candle
                _currentPeriod = period;
                _open = price;
                _high = price;
                _low = price;
                _close = price;
                return true;
            }

            // Update current candle
            _high = Math.Max(_high, price);
            _low = Math.Min(_low, price);
            _close = price;

            return false;
        }
    }

}