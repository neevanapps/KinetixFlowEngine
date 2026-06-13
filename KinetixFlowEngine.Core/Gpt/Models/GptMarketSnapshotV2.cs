namespace KinetixFlowEngine.Core.Gpt.Models;

public sealed class GptMarketSnapshotV2
{
    public int Sequence { get; init; }

    public string EngineVersion { get; init; } = string.Empty;

    public DateTime SnapshotTimeUtc { get; init; }

    public decimal Price { get; init; }

    public decimal VWAP { get; init; }

    public double ATR15m { get; init; }

    public double FundingRate { get; init; }

    public double FundingPressure { get; init; }

    public double OIChange { get; init; }

    // Arrays follow:
    // [10m,30m,60m]

    public double[] ScoreZ { get; init; } = [];

    public double[] VelocityZ { get; init; } = [];

    public double[] ImbalanceZ { get; init; } = [];

    public double[] CompressionZ { get; init; } = [];

    public double[] ExhaustionZ { get; init; } = [];

    public double[] Momentum { get; init; } = [];

    public double[] Acceleration { get; init; } = [];

    public double[] Persistence { get; init; } = [];

    public double[] NetPressure { get; init; } = [];

    public double[] FlowImpactEfficiency { get; init; } = [];

    public double[] ER5 { get; init; } = [];

    public double[] ER30 { get; init; } = [];
}