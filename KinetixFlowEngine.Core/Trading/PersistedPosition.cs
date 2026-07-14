using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Trading
{
    public class PersistedPosition
    {
        public string AccountId { get; set; } = default!;
        public string StrategyName { get; set; } = default!;
        public SignalDirection Direction { get; set; }

        public decimal EntryPrice { get; set; }
        public decimal StopLoss { get; set; }
        public decimal Target1 { get; set; }
        public decimal Size { get; set; }
        public long EntryTimeMs { get; set; }
        public bool Target1Hit { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
        public double EntryScoreZ { get; set; }
        public double EntryVelocityZ { get; set; }
        public double EntryImbalanceZ { get; set; }
        public double EntryCompressionZ { get; set; }
        public double EntryATR { get; set; }
        public double EntryER { get; set; }
        public string EntryFlowState { get; set; } = default!;
        public double EntryPriceTrend { get; set; }
        public double EntryScoreTrend { get; set; }
        public bool EntryAlertSent { get; set; }
        public bool MovedToBreakeven { get; set; }
        public decimal RemainingSize { get; set; }
        public decimal TrailingStop { get; set; }
        public string OrderId { get; set; } = string.Empty;

        public Guid? QuantIntentId { get; set; }
        public Guid? CurrentPayloadId { get; set; }
        public Guid? PreviousPayloadId { get; set; }
        public Guid? ThirdPayloadId { get; set; }
        public DateTimeOffset? ConsensusDecisionUtc { get; set; }
        public DateTimeOffset? SignalUtc { get; set; }
        public DateTimeOffset? PendingIntentCreatedUtc { get; set; }
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
        public decimal FairPriceAtEntry { get; set; }
        public double EntryDelaySeconds { get; set; }
        public string IntentExpiryReason { get; set; } = string.Empty;
    }
}
