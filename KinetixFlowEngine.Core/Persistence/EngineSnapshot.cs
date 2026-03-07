using KinetixFlowEngine.Core.Utils;

public class EngineSnapshot
{
    public DateTime Timestamp { get; set; }

    public double LastPrice { get; set; }

    public decimal PriceFastEma { get; set; }
    public decimal PriceSlowEma { get; set; }

    public decimal ScoreFastEma { get; set; }
    public decimal ScoreSlowEma { get; set; }

    public NormalizerState ScoreNormalizer { get; set; } = new();
    public NormalizerState VelocityNormalizer { get; set; } = new();
    public NormalizerState ImbalanceNormalizer { get; set; } = new();
    public NormalizerState ExhaustionNormalizer { get; set; } = new();
    public NormalizerState CompressionNormalizer { get; set; } = new();
}