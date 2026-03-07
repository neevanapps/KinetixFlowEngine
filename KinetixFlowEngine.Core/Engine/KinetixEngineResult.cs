using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Engine
{
    public class KinetixEngineResult
    {
        public double Price { get; set; }

        public double RawScore { get; set; }

        public double AdjustedScore { get; set; }

        public double ScoreZ { get; set; }
        public double VelocityZ { get; set; }
        public double ImbalanceZ { get; set; }
        public double ExhaustionZ { get; set; }
        public double CompressionZ { get; set; }

        public double VWAP { get; set; }
        public double ER { get; set; }
        public double ATR { get; set; }
        public double OIChange { get; set; }

        public FlowTrend PriceTrend { get; set; }
        public FlowTrend ScoreTrend { get; set; }

        public double ScoreFastEma { get; set; }
        public double ScoreMediumEma { get; set; }
        public double ScoreSlowEma { get; set; }

        public FlowStateSnapshot FlowState { get; set; } = new();

        public double LongProbability { get; set; }
        public double ShortProbability { get; set; }

        public bool LongStable { get; set; }
        public bool ShortStable { get; set; }

        public int LongPersistence { get; set; }
        public int ShortPersistence { get; set; }

        // NEW RAW FLOW FEATURES
        public double DeltaVelocity { get; set; }
        public double Momentum { get; set; }
        public double Acceleration { get; set; }
        public double Persistence { get; set; }
        public double SizeBias { get; set; }
        public double Absorption { get; set; }
    }
}