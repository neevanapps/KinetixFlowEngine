namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantModelConsensusDecision
{
    public bool IsAvailable { get; init; }

    public Guid CurrentPayloadId { get; init; }

    public Guid? PreviousPayloadId { get; init; }

    public Guid? ThirdPayloadId { get; init; }

    public DateTimeOffset DecisionTimeUtc { get; init; }

    public DateTimeOffset LatestCreatedUtc { get; init; }

    public string Direction { get; init; } = "NEUTRAL";

    public string RecommendedAction { get; init; } = "HOLD";

    public bool ShouldTrade { get; init; }

    // Temporal score across current / previous / third batches.
    public decimal WeightedDirectionalScore { get; init; }

    // Current-batch directional agreement, retained for strategy compatibility.
    public decimal AgreementRatio { get; init; }

    public string CurrentBatchDirection { get; init; } = "NEUTRAL";

    public string PreviousBatchDirection { get; init; } = "UNAVAILABLE";

    public string ThirdBatchDirection { get; init; } = "UNAVAILABLE";

    public decimal CurrentBatchWeightedDirectionalScore { get; init; }

    public decimal CurrentBatchDirectionalAgreementRatio { get; init; }

    public decimal CurrentBatchExecutableAgreementRatio { get; init; }

    public int CurrentBatchExecutableVoteCount { get; init; }

    public bool CurrentBatchShouldTrade { get; init; }

    public bool ThreeDirectionsAgree { get; init; }

    public int ExecutableBatchCount { get; init; }

    public decimal TemporalSpanMinutes { get; init; }

    public int LongVoteCount { get; init; }

    public int ShortVoteCount { get; init; }

    public int HoldVoteCount { get; init; }

    public int HighRiskVoteCount { get; init; }

    public int LowTradeabilityVoteCount { get; init; }

    public int ValidModelCount { get; init; }

    public int TotalModelCount { get; init; }

    public string ConsensusRiskLevel { get; init; } = "MEDIUM";

    public string ConsensusTradeability { get; init; } = "MEDIUM";

    public string Stability { get; init; } = "UNKNOWN";

    public string BlockReason { get; init; } = string.Empty;

    public IReadOnlyList<string> SupportingModels { get; init; } = [];

    public IReadOnlyList<string> ExecutableSupportingModels { get; init; } = [];

    public IReadOnlyList<string> OpposingModels { get; init; } = [];

    public IReadOnlyList<string> HoldModels { get; init; } = [];

    public IReadOnlyList<QuantModelDecision> ValidDecisions { get; init; } = [];

    public IReadOnlyList<QuantModelBatchConsensusDecision> BatchConsensuses { get; init; } = [];

    public static QuantModelConsensusDecision Unavailable(string reason)
    {
        return new QuantModelConsensusDecision
        {
            IsAvailable = false,
            Direction = "NEUTRAL",
            RecommendedAction = "HOLD",
            ShouldTrade = false,
            BlockReason = reason
        };
    }
}
