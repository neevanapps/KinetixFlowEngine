using KinetixFlowEngine.Core.Depth;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Models;

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
        KinetixEngineResult result)
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

            FlowImpactEfficiency = mtf.FlowImpactEfficiency,

            ER5 = mtf.ER5,

            ER30 = mtf.ER30,
            Depth = new GptDepthSnapshot
            {
                DepthImbalance = depthMtf.Imbalance,
                DepthBullPct = depthMtf.BullishPercent,
                BidWallAge = depthMtf.BidWallAge,
                AskWallAge = depthMtf.AskWallAge,
                BidWallQty = depthMtf.BidWallQty,
                AskWallQty = depthMtf.AskWallQty
            },
        };
    }
}