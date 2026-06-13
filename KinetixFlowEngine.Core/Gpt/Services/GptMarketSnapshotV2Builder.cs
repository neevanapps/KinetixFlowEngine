using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class GptMarketSnapshotV2Builder
{
    private readonly GptMultiTimeframeAggregator _aggregator;

    public GptMarketSnapshotV2Builder(
        GptMultiTimeframeAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    public GptMarketSnapshotV2 Build(
        int sequence,
        string engineVersion,
        KinetixEngineResult result)
    {
        var mtf = _aggregator.Build();

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

            ER30 = mtf.ER30
        };
    }
}