namespace KinetixFlowEngine.Core.Gpt.Models;

public sealed class GptMarketStateRow
{
    public DateTime TimestampUtc { get; init; }

    public double ScoreZ { get; init; }

    public double VelocityZ { get; init; }

    public double ImbalanceZ { get; init; }

    public double CompressionZ { get; init; }

    public double ExhaustionZ { get; init; }

    public double Momentum { get; init; }

    public double Acceleration { get; init; }

    public double Persistence { get; init; }

    public double NetPressure { get; init; }

    public double OIChange { get; init; }

    public double FundingPressure { get; init; }

    public double FlowImpactEfficiencyZ { get; init; }

    public double ER5 { get; init; }

    public double ER30 { get; init; }
}