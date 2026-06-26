using KinetixFlowEngine.Core.Domain.Common;
using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Flow;

public sealed class MinuteCandleBuilder
{
    private DateTime _currentMinute;

    private bool _hasTrades;

    private decimal _open;

    private decimal _high;

    private decimal _low;

    private decimal _close;

    public bool TryAddTrade(
        FlowTrade trade,
        out MinuteCandle? completed)
    {
        completed = null;

        var minute =
            DateTimeOffset
                .FromUnixTimeMilliseconds(trade.Timestamp)
                .UtcDateTime;

        minute = new DateTime(
            minute.Year,
            minute.Month,
            minute.Day,
            minute.Hour,
            minute.Minute,
            0,
            DateTimeKind.Utc);

        if (!_hasTrades)
        {
            StartNewMinute(minute, trade.Price);

            return false;
        }

        if (minute != _currentMinute)
        {
            completed = BuildCurrent();

            StartNewMinute(minute, trade.Price);

            return true;
        }

        Update(trade.Price);

        return false;
    }

    private void StartNewMinute(
        DateTime minute,
        decimal price)
    {
        _currentMinute = minute;

        _hasTrades = true;

        _open = price;

        _high = price;

        _low = price;

        _close = price;
    }

    private void Update(decimal price)
    {
        if (price > _high)
            _high = price;

        if (price < _low)
            _low = price;

        _close = price;
    }

    private MinuteCandle BuildCurrent()
    {
        return new MinuteCandle
        {
            MinuteUtc = _currentMinute,

            Open = _open,

            High = _high,

            Low = _low,

            Close = _close
        };
    }
}