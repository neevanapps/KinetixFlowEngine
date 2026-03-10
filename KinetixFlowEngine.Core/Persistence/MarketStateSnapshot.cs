using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Persistence
{
    public class MarketStateSnapshot
    {
        public int EngineVersion { get; set; } = 1;

        public DateTime Timestamp { get; set; }

        public double LastPrice { get; set; }

        public decimal PriceFastEma { get; set; }
        public decimal PriceSlowEma { get; set; }

        public decimal ScoreFastEma { get; set; }
        public decimal ScoreSlowEma { get; set; }
        public decimal ScoreMediumEma { get; set; }

        public decimal ProbSlowEma { get; set; }
        public decimal ProbMediumEma { get; set; }
        public decimal ProbFastEma { get; set; }

        public NormalizerState ScoreNormalizer { get; set; } = new();
        public NormalizerState VelocityNormalizer { get; set; } = new();
        public NormalizerState ImbalanceNormalizer { get; set; } = new();
        public NormalizerState ExhaustionNormalizer { get; set; } = new();
        public NormalizerState CompressionNormalizer { get; set; } = new();
    }
}