namespace KinetixFlowEngine.Core.Gpt.Models;

public sealed class GptMarketSnapshot
{
    // Metadata
    public int Sequence { get; init; }

    public string EngineVersion { get; init; } = string.Empty;

    public DateTime StartTimeUtc { get; init; }

    public DateTime EndTimeUtc { get; init; }

    public int SampleCount { get; init; }

    // Candle Structure
    public decimal Open { get; init; }

    public decimal High { get; init; }

    public decimal Low { get; init; }

    public decimal Close { get; init; }

    // Market Context
    public double VWAP { get; init; }

    public double ATR15m { get; init; }

    public double OIStart { get; init; }

    public double OIEnd { get; init; }

    public double FundingRate { get; init; }

    public double FundingPressure { get; init; }

    // Score Layer
    public double RawScore { get; init; }

    public double AdjustedScore { get; init; }

    public double ScoreZ { get; init; }

    public double VelocityZ { get; init; }

    public double ImbalanceZ { get; init; }

    public double CompressionZ { get; init; }

    public double ExhaustionZ { get; init; }

    // Flow Layer (10 min averages)
    public double MomentumAvg { get; init; }

    public double AccelerationAvg { get; init; }

    public double PersistenceAvg { get; init; }

    public double SizeBiasAvg { get; init; }

    public double AbsorptionAvg { get; init; }

    public double DeltaVelocityAvg { get; init; }

    // Pressure Layer (10 min averages)
    public double BuyPressureAvg { get; init; }

    public double SellPressureAvg { get; init; }

    public double NetPressureAvg { get; init; }

    // Whale Layer
    public int LargeBuyTrades { get; init; }

    public int LargeSellTrades { get; init; }

    public double BuyClusterStrength { get; init; }

    public double SellClusterStrength { get; init; }

    public double FlowImpactEfficiency { get; init; }

    // Participation
    public double Volume15 { get; init; }

    // Trend Efficiency
    public double ER5 { get; init; }

    public double ER30 { get; init; }
}