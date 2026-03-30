using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;

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
        public double ER5 { get; set; }
        public double ER30 { get; set; }
        public double ATR { get; set; }
        public double OIChange { get; set; }
        public double ATR15m { get; set; }

        public FlowTrend PriceTrend { get; set; }
        public FlowTrend ScoreTrend { get; set; }

        public double ScoreFastEma { get; set; }
        public double ScoreMediumEma { get; set; }
        public double ScoreSlowEma { get; set; }

        public double ProbFastEma { get; set; }
        public double ProbMediumEma { get; set; }
        public double ProbSlowEma { get; set; }

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

        public bool BullishAbsorption { get; set; }
        public bool BearishDistribution { get; set; }
        public double DivergenceStrength { get; set; }

        public double BuyPressure { get; set; }
        public double SellPressure { get; set; }
        public double NetPressure { get; set; }

        public bool BullishBreakout { get; set; }
        public bool BearishBreakout { get; set; }

        public bool VwapBullishAbsorption { get; set; }
        public bool VwapBearishAbsorption { get; set; }
        public double VwapAbsorptionStrength { get; set; }

        public int LargeBuyTrades { get; set; }
        public int LargeSellTrades { get; set; }

        public double BuyClusterStrength { get; set; }
        public double SellClusterStrength { get; set; }
        public int BullishPersistence { get; set; }
        public int BearishPersistence { get; set; }

        public bool StrongBullishPersistence { get; set; }
        public bool StrongBearishPersistence { get; set; }
        public double FlowImpactEfficiency { get; set; }

        public bool BullishPriceControl { get; set; }
        public bool BearishPriceControl { get; set; }

        public double Volume15 { get; set; }
        public double Volume1 { get; set; }

        public double TrendFactor { get; set; }
        public double VelocityEma { get; set; }
        public bool MomentumDying { get; set; }
        public bool TradeGate { get; set; }
        public double FundingRate { get; set; }
        public double FundingPressure { get; set; }
        public double AtrNorm { get; set; }
        public EmaStabilityState EmaStability { get; set; } = new();
    }
}