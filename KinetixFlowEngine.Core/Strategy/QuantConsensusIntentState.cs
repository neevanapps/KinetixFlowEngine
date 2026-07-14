namespace KinetixFlowEngine.Core.Strategy;

public sealed class QuantConsensusIntentState
{
    public Guid IntentId { get; set; }

    public string StrategyName { get; set; } = string.Empty;

    public SignalDirection Direction { get; set; }

    public string Status { get; set; } = "PENDING";

    public Guid CurrentPayloadId { get; set; }

    public Guid? PreviousPayloadId { get; set; }

    public Guid? ThirdPayloadId { get; set; }

    public DateTimeOffset ConsensusDecisionUtc { get; set; }

    public DateTimeOffset SignalUtc { get; set; }

    public DateTimeOffset PendingIntentCreatedUtc { get; set; }

    public DateTimeOffset? EntryUtc { get; set; }

    public int ReviewCount { get; set; }

    public decimal CurrentBatchScore { get; set; }

    public decimal TemporalScore { get; set; }

    public int ExecutableVotes { get; set; }

    public decimal DirectionalAgreement { get; set; }

    public decimal ExecutableAgreement { get; set; }

    public int ExecutableBatchCount { get; set; }

    public decimal ReviewSpanMinutes { get; set; }

    public decimal MarketPriceAtSignal { get; set; }

    public decimal FairPriceAtSignal { get; set; }

    public decimal? EntryPrice { get; set; }

    public decimal? FairPriceAtEntry { get; set; }

    public double? EntryDelaySeconds { get; set; }

    public string IntentExpiryReason { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public string OrderId { get; set; } = string.Empty;

    public QuantConsensusIntentState Clone()
    {
        return new QuantConsensusIntentState
        {
            IntentId = IntentId,
            StrategyName = StrategyName,
            Direction = Direction,
            Status = Status,
            CurrentPayloadId = CurrentPayloadId,
            PreviousPayloadId = PreviousPayloadId,
            ThirdPayloadId = ThirdPayloadId,
            ConsensusDecisionUtc = ConsensusDecisionUtc,
            SignalUtc = SignalUtc,
            PendingIntentCreatedUtc = PendingIntentCreatedUtc,
            EntryUtc = EntryUtc,
            ReviewCount = ReviewCount,
            CurrentBatchScore = CurrentBatchScore,
            TemporalScore = TemporalScore,
            ExecutableVotes = ExecutableVotes,
            DirectionalAgreement = DirectionalAgreement,
            ExecutableAgreement = ExecutableAgreement,
            ExecutableBatchCount = ExecutableBatchCount,
            ReviewSpanMinutes = ReviewSpanMinutes,
            MarketPriceAtSignal = MarketPriceAtSignal,
            FairPriceAtSignal = FairPriceAtSignal,
            EntryPrice = EntryPrice,
            FairPriceAtEntry = FairPriceAtEntry,
            EntryDelaySeconds = EntryDelaySeconds,
            IntentExpiryReason = IntentExpiryReason,
            AccountId = AccountId,
            OrderId = OrderId
        };
    }
}
