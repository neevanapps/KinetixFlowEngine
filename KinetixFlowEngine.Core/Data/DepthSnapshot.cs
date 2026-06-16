namespace KinetixFlowEngine.Core.Data;

public sealed class DepthSnapshot
{
    public DateTime TimestampUtc { get; init; }

    public IReadOnlyList<DepthLevel> Bids { get; init; }
        = Array.Empty<DepthLevel>();

    public IReadOnlyList<DepthLevel> Asks { get; init; }
        = Array.Empty<DepthLevel>();
}

public sealed class DepthLevel
{
    public decimal Price { get; init; }

    public decimal Quantity { get; init; }
}