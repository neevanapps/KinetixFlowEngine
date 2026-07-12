namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantModelDecision
{
    public Guid DecisionId { get; init; }

    public Guid PayloadId { get; init; }

    public string Symbol { get; init; } = "BTCUSDT";

    public DateTimeOffset DecisionTimeUtc { get; init; }

    public DateTimeOffset CreatedUtc { get; init; }

    public string ModelName { get; init; } = string.Empty;

    public string Provider { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string BiasDirection { get; init; } = "Unknown";

    public string RecommendedAction { get; init; } = "HOLD";

    public bool ShouldTrade { get; init; }

    public int LongConfidence { get; init; }

    public int ShortConfidence { get; init; }

    public int DirectionalScore { get; init; }

    public string RiskLevel { get; init; } = "Unknown";

    public string Tradeability { get; init; } = "MEDIUM";

    public string DominantIntent { get; init; } = "UNCLEAR";

    public int TimeHorizonMinutes { get; init; }

    public string DecisionReason { get; init; } = string.Empty;

    public IReadOnlyList<string> SupportingEvidence { get; init; } = [];

    public IReadOnlyList<string> OpposingEvidence { get; init; } = [];

    public IReadOnlyList<string> InvalidationConditions { get; init; } = [];

    public string RawResponseJson { get; init; } = "{}";

    public string ParsedResponseJson { get; init; } = "{}";

    public string ErrorMessage { get; init; } = string.Empty;

    public int LatencyMs { get; init; }

    public bool IsSuccess =>
        Status.Equals("Success", StringComparison.OrdinalIgnoreCase);

    public bool IsLong =>
        BiasDirection.Equals("Long", StringComparison.OrdinalIgnoreCase) ||
        BiasDirection.Equals("LONG", StringComparison.OrdinalIgnoreCase);

    public bool IsShort =>
        BiasDirection.Equals("Short", StringComparison.OrdinalIgnoreCase) ||
        BiasDirection.Equals("SHORT", StringComparison.OrdinalIgnoreCase);
}