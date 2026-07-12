namespace KinetixFlowEngine.Core.Strategy.Strategies;

public sealed class QuantModelConsensusStrategyOptions
{
    // Global switch for all Quant consensus review strategies.
    public bool Enabled { get; set; } = true;

    public int MaxConsensusAgeSeconds { get; set; } = 900;

    // Retained for compatibility with the previous QuantModelConsensusStrategy.
    public bool RequireShouldTradeForEntry { get; set; } = true;

    public bool EnableExitOnOppositeConsensus { get; set; } = true;

    public bool RequireShouldTradeForExit { get; set; } = false;

    public int MinExitDirectionalScore { get; set; } = 30;

    public decimal MinExitAgreementRatio { get; set; } = 0.60m;

    public bool LogNoSignalReason { get; set; } = false;

    public QuantConsensusOneReviewOptions OneReview { get; set; } = new();

    public QuantConsensusTwoReviewOptions TwoReview { get; set; } = new();

    public QuantConsensusThreeReviewOptions ThreeReview { get; set; } = new();
}

public sealed class QuantConsensusOneReviewOptions
{
    public bool Enabled { get; set; } = true;

    public int MinDirectionalScore { get; set; } = 35;

    public int MinExecutableVotes { get; set; } = 3;

    public bool RequireBatchShouldTrade { get; set; } = true;
}

public sealed class QuantConsensusTwoReviewOptions
{
    public bool Enabled { get; set; } = true;

    public int MinCurrentDirectionalScore { get; set; } = 30;

    public int MinWeightedDirectionalScore { get; set; } = 30;

    public int MinExecutableVotes { get; set; } = 3;

    public bool RequireBothBatchesExecutable { get; set; } = true;

    public int MaxReviewSpanMinutes { get; set; } = 180;

    public decimal CurrentBatchWeight { get; set; } = 0.65m;

    public decimal PreviousBatchWeight { get; set; } = 0.35m;
}

public sealed class QuantConsensusThreeReviewOptions
{
    public bool Enabled { get; set; } = true;

    public int MinWeightedDirectionalScore { get; set; } = 25;

    public int MinExecutableVotes { get; set; } = 3;

    public int MinExecutableBatchCount { get; set; } = 2;

    public int MaxReviewSpanMinutes { get; set; } = 180;

    public decimal CurrentBatchWeight { get; set; } = 0.50m;

    public decimal PreviousBatchWeight { get; set; } = 0.30m;

    public decimal ThirdBatchWeight { get; set; } = 0.20m;
}
