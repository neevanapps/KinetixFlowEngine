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

    public GptDepthSnapshot Depth { get; init; } = new();

    public string Trend10m { get; init; } = string.Empty;

    public string Trend30m { get; init; } = string.Empty;

    public string Trend60m { get; init; } = string.Empty;

    public double DistanceFrom10mHigh { get; init; }

    public double DistanceFrom10mLow { get; init; }

    public double DistanceFrom30mHigh { get; init; }

    public double DistanceFrom30mLow { get; init; }

    public double DistanceFrom60mHigh { get; init; }

    public double DistanceFrom60mLow { get; init; }

    public decimal Open10m { get; init; }
    public decimal High10m { get; init; }
    public decimal Low10m { get; init; }
    public decimal Close10m { get; init; }

    public decimal Open30m { get; init; }
    public decimal High30m { get; init; }
    public decimal Low30m { get; init; }
    public decimal Close30m { get; init; }

    public decimal Open60m { get; init; }
    public decimal High60m { get; init; }
    public decimal Low60m { get; init; }
    public decimal Close60m { get; init; }

    public double BodyPct10m { get; init; }
    public double UpperWickPct10m { get; init; }
    public double LowerWickPct10m { get; init; }

    public double BodyPct30m { get; init; }
    public double UpperWickPct30m { get; init; }
    public double LowerWickPct30m { get; init; }

    public double BodyPct60m { get; init; }
    public double UpperWickPct60m { get; init; }
    public double LowerWickPct60m { get; init; }

    public double DistanceFromVWAP { get; init; }

    public double DistanceFromVWAPPct { get; init; }

    public decimal RangeHigh10m { get; init; }
    public decimal RangeLow10m { get; init; }

    public decimal RangeHigh30m { get; init; }
    public decimal RangeLow30m { get; init; }

    public decimal RangeHigh60m { get; init; }
    public decimal RangeLow60m { get; init; }
}