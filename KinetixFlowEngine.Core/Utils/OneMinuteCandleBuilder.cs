public class OneMinuteCandleBuilder
{
    private long _currentMinute = -1;

    private double _high;
    private double _low;
    private double _close;

    public bool Update(double price, long timestampMs, out (double High, double Low, double Close) candle)
    {
        candle = default;

        long minute = timestampMs / 60000; // minute bucket

        if (_currentMinute == -1)
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