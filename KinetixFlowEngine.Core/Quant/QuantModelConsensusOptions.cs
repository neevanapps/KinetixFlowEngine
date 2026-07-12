namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantModelConsensusOptions
{
    public bool Enabled { get; set; } = true;

    public int MinValidModelCount { get; set; } = 3;

    public decimal MinDirectionalAgreementRatio { get; set; } = 0.60m;

    public int MinExecutableVotes { get; set; } = 3;

    public int MinBatchDirectionalScore { get; set; } = 25;

    public int MinTemporalDirectionalScore { get; set; } = 25;

    public int MinExecutableBatchCount { get; set; } = 2;

    public bool RequireThreeMatchingDirections { get; set; } = true;

    public int MaxThreeBatchSpanMinutes { get; set; } = 180;

    public decimal CurrentBatchWeight { get; set; } = 0.50m;

    public decimal PreviousBatchWeight { get; set; } = 0.30m;

    public decimal ThirdBatchWeight { get; set; } = 0.20m;

    public bool BlockHighRisk { get; set; } = true;

    public bool BlockLowTradeability { get; set; } = true;

    public Dictionary<string, decimal> ModelWeights { get; set; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["qwen"] = 1.00m,
            ["mistral"] = 1.00m,
            ["gpt"] = 1.00m,
            ["glm"] = 1.00m,
            ["llama"] = 1.00m,
            ["openrouter"] = 1.00m,
            ["groq"] = 1.00m
        };
}
