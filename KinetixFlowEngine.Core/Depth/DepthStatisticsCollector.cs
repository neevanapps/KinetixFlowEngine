using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Domain.Liquidity;

namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthStatisticsCollector
{
    private readonly DepthWallTracker _wallTracker;

    private DateTime _currentMinute;
    private DateTime _startUtc;
    private DateTime _endUtc;


    private bool _hasData;

    private readonly List<DepthSnapshot> _snapshots = new();

    public DepthStatisticsCollector(
        DepthWallTracker wallTracker)
    {
        _wallTracker = wallTracker;
    }

    public bool TryAddSnapshot(
    DepthSnapshot snapshot,
    out DepthMinuteSnapshot? completedMinute)
    {
        completedMinute = null;

        var minute = new DateTime(
            snapshot.TimestampUtc.Year,
            snapshot.TimestampUtc.Month,
            snapshot.TimestampUtc.Day,
            snapshot.TimestampUtc.Hour,
            snapshot.TimestampUtc.Minute,
            0,
            DateTimeKind.Utc);

        if (!_hasData)
        {
            _currentMinute = minute;
            _hasData = true;

            AddSnapshot(snapshot);

            return false;
        }

        if (minute != _currentMinute)
        {
            completedMinute = new DepthMinuteSnapshot
            {
                MinuteUtc = _currentMinute,
                StartUtc = _startUtc,
                EndUtc = _endUtc,
                Snapshots = _snapshots.ToList()
            };

            Reset();

            _currentMinute = minute;

            _hasData = true;

            AddSnapshot(snapshot);

            return true;
        }

        AddSnapshot(snapshot);

        return false;
    }

    private void AddSnapshot(
        DepthSnapshot snapshot)
    {
        if (_snapshots.Count == 0)
        {
            _startUtc = snapshot.TimestampUtc;
        }

        _endUtc = snapshot.TimestampUtc;

        _snapshots.Add(snapshot);

        _wallTracker.Update(snapshot);
    }

    private void Reset()
    {
        _snapshots.Clear();

        _startUtc = default;

        _endUtc = default;
    }
}