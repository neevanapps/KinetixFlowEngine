namespace KinetixFlowEngine.Core.Strategy.Strategies;

internal sealed class QuantConsensusEntryEvaluation
{
    public bool Approved { get; init; }

    public string Direction { get; init; } = "NEUTRAL";

    public decimal Score { get; init; }

    public decimal CurrentBatchScore { get; init; }

    public int ReviewCount { get; init; }

    public decimal ReviewSpanMinutes { get; init; }

    public int ExecutableBatchCount { get; init; }

    public int CurrentExecutableVoteCount { get; init; }

    public decimal CurrentDirectionalAgreementRatio { get; init; }

    public decimal CurrentExecutableAgreementRatio { get; init; }

    public Guid? CurrentPayloadId { get; init; }

    public Guid? PreviousPayloadId { get; init; }

    public Guid? ThirdPayloadId { get; init; }

    public DateTimeOffset ConsensusDecisionUtc { get; init; }

    public string BlockReason { get; init; } = string.Empty;

    public static QuantConsensusEntryEvaluation Blocked(
        int reviewCount,
        string reason)
    {
        return new QuantConsensusEntryEvaluation
        {
            Approved = false,
            ReviewCount = reviewCount,
            BlockReason = reason
        };
    }
}
