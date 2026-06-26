namespace KinetixFlowEngine.Core.Domain.Common;

public sealed class MarketEvent
{
    public DateTime TimestampUtc { get; init; }

    public MarketEventType Type { get; init; }

    public MarketStrength Strength { get; init; }

    public decimal Value { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}