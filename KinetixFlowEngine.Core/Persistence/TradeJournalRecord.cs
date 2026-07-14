using KinetixFlowEngine.Core.Strategy;

public class TradeJournalRecord
{
    public string TradeId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Strategy { get; set; } = default!;
    public SignalDirection Direction { get; set; }

    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal Target1 { get; set; }
    public bool Target1Hit { get; set; }
    public decimal Size { get; set; }

    public double DurationSeconds { get; set; }

    public decimal PnlUsd { get; set; }
    public decimal GrossPnlUsd { get; set; }
    public decimal FeeUsd { get; set; }
    public decimal PnlR { get; set; }

    public decimal MFE { get; set; }
    public decimal MAE { get; set; }

    public double ScoreZ { get; set; }
    public double VelocityZ { get; set; }
    public double ImbalanceZ { get; set; }
    public double CompressionZ { get; set; }

    public decimal ATR { get; set; }
    public decimal ER { get; set; }

    public string FlowState { get; set; } = default!;

    public string AccountId { get; set; } = string.Empty;
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
    public string ExitReason { get; set; } = string.Empty;

    public decimal ConfiguredLeverage { get; set; } = 1m;
    public decimal LeveragedGrossPnlUsd { get; set; }
    public decimal LeveragedFeeUsd { get; set; }
    public decimal LeveragedPnlUsd { get; set; }
}
