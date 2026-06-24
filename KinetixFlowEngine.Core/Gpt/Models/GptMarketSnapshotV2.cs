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
    // [Level1,Level2,Level3]

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

    public GptDepthSnapshot Depth { get; init; } = new();

    public string TrendLevel1 { get; init; } = string.Empty;

    public string TrendLevel2 { get; init; } = string.Empty;

    public string TrendLevel3 { get; init; } = string.Empty;

    public double DistanceFromLevel1High { get; init; }

    public double DistanceFromLevel1Low { get; init; }

    public double DistanceFromLevel2High { get; init; }

    public double DistanceFromLevel2Low { get; init; }

    public double DistanceFromLevel3High { get; init; }

    public double DistanceFromLevel3Low { get; init; }


    public double BodyPctLevel1 { get; init; }
    public double UpperWickPctLevel1 { get; init; }
    public double LowerWickPctLevel1 { get; init; }

    public double BodyPctLevel2 { get; init; }
    public double UpperWickPctLevel2 { get; init; }
    public double LowerWickPctLevel2 { get; init; }

    public double BodyPctLevel3 { get; init; }
    public double UpperWickPctLevel3 { get; init; }
    public double LowerWickPctLevel3 { get; init; }

    public double DistanceFromVWAP { get; init; }

    public double DistanceFromVWAPPct { get; init; }
    public string HistorySummary { get; init; } = string.Empty;
}