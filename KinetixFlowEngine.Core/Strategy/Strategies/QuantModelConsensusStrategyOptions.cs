namespace KinetixFlowEngine.Core.Strategy.Strategies;

public sealed class QuantModelConsensusStrategyOptions
{
    public bool Enabled { get; set; } = true;

    public int MaxConsensusAgeSeconds { get; set; } = 900;

    public bool RequireShouldTradeForEntry { get; set; } = true;

    public bool EnableExitOnOppositeConsensus { get; set; } = true;

    public bool RequireShouldTradeForExit { get; set; } = false;

    public int MinExitDirectionalScore { get; set; } = 30;

    public decimal MinExitAgreementRatio { get; set; } = 0.60m;

    public bool LogNoSignalReason { get; set; } = false;
}