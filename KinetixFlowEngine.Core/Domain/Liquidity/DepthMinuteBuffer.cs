namespace KinetixFlowEngine.Core.Domain.Liquidity;

public sealed class DepthMinuteBuffer
{
    private readonly List<DepthSnapshot> _snapshots = new();

    public void AddSnapshot(
        DepthSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        _snapshots.Add(snapshot);
    }

    public DepthMinuteSnapshot CompleteMinute(
        DateTime minuteUtc)
    {
        var result = new DepthMinuteSnapshot
        {
            MinuteUtc = minuteUtc,
            StartUtc = minuteUtc,
            EndUtc = minuteUtc.AddMinutes(1),
            Snapshots = _snapshots.ToArray()
        };

        _snapshots.Clear();

        return result;
    }
}