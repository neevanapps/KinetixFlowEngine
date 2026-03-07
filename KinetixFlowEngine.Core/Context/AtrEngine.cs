public class AtrEngine
{
    private const int Period = 14;

    private double _atr;
    private double _prevClose;
    private int _count;

    public double Value => _atr;

    public bool IsReady => _count >= Period;

    public void Reset()
    {
        _atr = 0;
        _prevClose = 0;
        _count = 0;
    }

    public double Update(double high, double low, double close)
    {
        double tr;

        if (_count == 0)
        {
            tr = high - low;
        }
        else
        {
            var r1 = high - low;
            var r2 = Math.Abs(high - _prevClose);
            var r3 = Math.Abs(low - _prevClose);

            tr = Math.Max(r1, Math.Max(r2, r3));
        }

        _count++;

        if (_count <= Period)
        {
            _atr += tr;

            if (_count == Period)
                _atr /= Period;
        }
        else
        {
            _atr = ((_atr * (Period - 1)) + tr) / Period;
        }

        _prevClose = close;

        return _atr;
    }
}