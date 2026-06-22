using KinetixFlowEngine.Core.Depth;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Models;
using Serilog.Core;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class GptMarketSnapshotV2Builder
{
    private readonly GptMultiTimeframeAggregator _aggregator;
    private readonly DepthFeatureManager _depthFeatureManager;


    public GptMarketSnapshotV2Builder(
        GptMultiTimeframeAggregator aggregator,
    DepthFeatureManager depthFeatureManager)
    {
        _aggregator = aggregator;
        _depthFeatureManager = depthFeatureManager;
    }

    public GptMarketSnapshotV2 Build(
    int sequence,
    string engineVersion,
    KinetixEngineResult result,
    MarketStructureSnapshot structure)

    {
        var mtf = _aggregator.Build();
        var depthMtf = new DepthMtfAggregator(_depthFeatureManager.Rows).Build();

        return new GptMarketSnapshotV2
        {
            Sequence = sequence,

            EngineVersion = engineVersion,

            SnapshotTimeUtc = DateTime.UtcNow,

            Price = (decimal)result.Price,

            VWAP = (decimal)result.VWAP,

            ATR15m = result.ATR15m,

            FundingRate = result.FundingRate,

            FundingPressure = result.FundingPressure,

            OIChange = result.OIChange,

            ScoreZ = mtf.ScoreZ,

            VelocityZ = mtf.VelocityZ,

            ImbalanceZ = mtf.ImbalanceZ,

            CompressionZ = mtf.CompressionZ,

            ExhaustionZ = mtf.ExhaustionZ,

            Momentum = mtf.Momentum,

            Acceleration = mtf.Acceleration,

            Persistence = mtf.Persistence,

            NetPressure = mtf.NetPressure,

            FlowImpactEfficiency = mtf.FlowImpactEfficiencyZ,

            ER5 = mtf.ER5,

            ER30 = mtf.ER30,
            Depth = new GptDepthSnapshot
            {
                DepthImbalance = depthMtf.Imbalance,
                DepthBullPct = depthMtf.BullishPercent,
                BidWallAge = depthMtf.BidWallAge,
                AskWallAge = depthMtf.AskWallAge,
                BidWallQty = depthMtf.BidWallQty,
                AskWallQty = depthMtf.AskWallQty,
                BidConsumption = depthMtf.BidConsumption,
                BullishPersistence = depthMtf.BullishPersistence,
                AskConsumption = depthMtf.AskConsumption
            },
            Trend10m = structure.Trend10m,
            Trend30m = structure.Trend30m,
            Trend60m = structure.Trend60m,

            DistanceFrom10mHigh = structure.DistanceFrom10mHigh,
            DistanceFrom10mLow = structure.DistanceFrom10mLow,

            DistanceFrom30mHigh = structure.DistanceFrom30mHigh,
            DistanceFrom30mLow = structure.DistanceFrom30mLow,

            DistanceFrom60mHigh = structure.DistanceFrom60mHigh,
            DistanceFrom60mLow = structure.DistanceFrom60mLow,

            DistanceFromVWAP = structure.DistanceFromVWAP,
            DistanceFromVWAPPct = structure.DistanceFromVWAPPct,

            Open10m = structure.Candle10m.Open,
            High10m = structure.Candle10m.High,
            Low10m = structure.Candle10m.Low,
            Close10m = structure.Candle10m.Close,

            Open30m = structure.Candle30m.Open,
            High30m = structure.Candle30m.High,
            Low30m = structure.Candle30m.Low,
            Close30m = structure.Candle30m.Close,

            Open60m = structure.Candle60m.Open,
            High60m = structure.Candle60m.High,
            Low60m = structure.Candle60m.Low,
            Close60m = structure.Candle60m.Close,

            BodyPct10m = structure.Candle10m.BodyPct,
            UpperWickPct10m = structure.Candle10m.UpperWickPct,
            LowerWickPct10m = structure.Candle10m.LowerWickPct,

            BodyPct30m = structure.Candle30m.BodyPct,
            UpperWickPct30m = structure.Candle30m.UpperWickPct,
            LowerWickPct30m = structure.Candle30m.LowerWickPct,

            BodyPct60m = structure.Candle60m.BodyPct,
            UpperWickPct60m = structure.Candle60m.UpperWickPct,
            LowerWickPct60m = structure.Candle60m.LowerWickPct,

            RangeHigh10m = structure.RangeHigh10m,
            RangeLow10m = structure.RangeLow10m,

            RangeHigh30m = structure.RangeHigh30m,
            RangeLow30m = structure.RangeLow30m,

            RangeHigh60m = structure.RangeHigh60m,
            RangeLow60m = structure.RangeLow60m,
        };
    }
}