namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantModelBatchConsensusDecision
{
    public Guid PayloadId { get; init; }

    public DateTimeOffset DecisionTimeUtc { get; init; }

    public DateTimeOffset LatestCreatedUtc { get; init; }

    public string CompletionMode { get; init; } = "UNKNOWN";

    public string Direction { get; init; } = "NEUTRAL";

    public decimal WeightedDirectionalScore { get; init; }

    public decimal DirectionalAgreementRatio { get; init; }

    public decimal ExecutableAgreementRatio { get; init; }

    public int LongDirectionalVoteCount { get; init; }

    public int ShortDirectionalVoteCount { get; init; }

    public int LongExecutableVoteCount { get; init; }

    public int ShortExecutableVoteCount { get; init; }

    public int ExecutableVoteCount { get; init; }

    public int HoldVoteCount { get; init; }

    public int HighRiskVoteCount { get; init; }

    public int LowTradeabilityVoteCount { get; init; }

    public int ValidModelCount { get; init; }

    public int TotalModelCount { get; init; }

    public string RiskLevel { get; init; } = "MEDIUM";

    public string Tradeability { get; init; } = "MEDIUM";

    public bool ShouldTrade { get; init; }

    public string BlockReason { get; init; } = string.Empty;

    public IReadOnlyList<string> DirectionalSupportingModels { get; init; } = [];

    public IReadOnlyList<string> ExecutableSupportingModels { get; init; } = [];

    public IReadOnlyList<string> OpposingModels { get; init; } = [];

    public IReadOnlyList<string> HoldModels { get; init; } = [];

    public IReadOnlyList<QuantModelDecision> ValidDecisions { get; init; } = [];
}
