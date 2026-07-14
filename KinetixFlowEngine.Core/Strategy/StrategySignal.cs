namespace KinetixFlowEngine.Core.Strategy
{
    public class StrategySignal
    {
        public string StrategyName { get; set; } = "";

        public SignalDirection Direction { get; set; }

        public double Confidence { get; set; }

        public bool EnterOnlyAtFairPrice { get; set; }

        public bool NotifyThroughTelegram { get; set; }

        public decimal? SuggestedEntryPrice { get; set; }

        public bool FairPriceApproved { get; set; }

        public bool IsVolumeBased { get; set; }

        public bool ExitSignal { get; set; }

        public decimal RiskPercent { get; set; } = 0.01m;

        public List<string> TargetAccountIds { get; set; } = new();

        // Quant decision lineage and paper-strategy experiment metadata.
        public Guid? QuantIntentId { get; set; }

        public Guid? CurrentPayloadId { get; set; }

        public Guid? PreviousPayloadId { get; set; }

        public Guid? ThirdPayloadId { get; set; }

        public DateTimeOffset? ConsensusDecisionUtc { get; set; }

        public DateTimeOffset? SignalUtc { get; set; }

        public DateTimeOffset? PendingIntentCreatedUtc { get; set; }

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

        public StrategySignal Clone()
        {
            return new StrategySignal
            {
                StrategyName = StrategyName,
                Direction = Direction,
                Confidence = Confidence,
                EnterOnlyAtFairPrice = EnterOnlyAtFairPrice,
                NotifyThroughTelegram = NotifyThroughTelegram,
                SuggestedEntryPrice = SuggestedEntryPrice,
                FairPriceApproved = FairPriceApproved,
                IsVolumeBased = IsVolumeBased,
                ExitSignal = ExitSignal,
                RiskPercent = RiskPercent,
                TargetAccountIds = new List<string>(TargetAccountIds),
                QuantIntentId = QuantIntentId,
                CurrentPayloadId = CurrentPayloadId,
                PreviousPayloadId = PreviousPayloadId,
                ThirdPayloadId = ThirdPayloadId,
                ConsensusDecisionUtc = ConsensusDecisionUtc,
                SignalUtc = SignalUtc,
                PendingIntentCreatedUtc = PendingIntentCreatedUtc,
                ReviewCount = ReviewCount,
                CurrentBatchScore = CurrentBatchScore,
                TemporalScore = TemporalScore,
                ExecutableVotes = ExecutableVotes,
                DirectionalAgreement = DirectionalAgreement,
                ExecutableAgreement = ExecutableAgreement,
                ExecutableBatchCount = ExecutableBatchCount,
                ReviewSpanMinutes = ReviewSpanMinutes,
                MarketPriceAtSignal = MarketPriceAtSignal,
                FairPriceAtSignal = FairPriceAtSignal
            };
        }
    }
}
