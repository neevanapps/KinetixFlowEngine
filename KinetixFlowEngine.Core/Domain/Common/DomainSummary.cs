namespace KinetixFlowEngine.Core.Domain.Common;

public sealed class DomainSummary
{
    public MarketBias Bias { get; init; }

    public MarketStrength Strength { get; init; }

    public string Narrative { get; init; } = string.Empty;
}