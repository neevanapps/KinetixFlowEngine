namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantModelDecisionBatch
{
    public Guid PayloadId { get; init; }

    public string Symbol { get; init; } = "BTCUSDT";

    public DateTimeOffset DecisionTimeUtc { get; init; }

    public DateTimeOffset LatestCreatedUtc { get; init; }

    public DateTimeOffset CompletionUtc { get; init; }

    public string CompletionMode { get; init; } = "UNKNOWN";

    public int ExpectedModelCount { get; init; }

    public int ObservedExpectedModelCount { get; init; }

    public int TerminalModelCount { get; init; }

    public int SuccessfulModelCount { get; init; }

    public bool IsComplete { get; init; }

    public IReadOnlyList<string> MissingExpectedModels { get; init; } = [];

    public IReadOnlyList<string> UnexpectedModels { get; init; } = [];

    public IReadOnlyList<QuantModelDecision> Decisions { get; init; } = [];

    public int TotalDecisionCount => Decisions.Count;

    public int ValidDecisionCount => Decisions.Count(x => x.IsSuccess);

    public bool HasEnoughValidModels(int minValidModelCount) =>
        ValidDecisionCount >= minValidModelCount;

    public bool IsStale(TimeSpan maxAge) =>
        DateTimeOffset.UtcNow - DecisionTimeUtc > maxAge;
}
