using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Liquidity;

public sealed class DepthBuilder
{
    private readonly MetricSeriesBuilder _seriesBuilder;

    public DepthBuilder(
        MetricSeriesBuilder seriesBuilder)
    {
        _seriesBuilder = seriesBuilder;
    }

    public Depth Build(DepthMinuteSnapshot minute)
    {
        var snapshots = minute.Snapshots;

        return new Depth
        {
            Liquidity = BuildLiquidity(snapshots),

            Walls = BuildWalls(snapshots),

            Consumption = BuildConsumption(snapshots),

            Spread = BuildSpread(snapshots),

            Imbalance = BuildImbalance(snapshots)
        };
    }

    private LiquidityVolume BuildLiquidity(
     IReadOnlyList<DepthSnapshot> snapshots)
    {
        return new LiquidityVolume
        {
            Bid = _seriesBuilder.Build(
                snapshots,
                x => x.TotalBidLiquidity),

            Ask = _seriesBuilder.Build(
                snapshots,
                x => x.TotalAskLiquidity)
        };
    }

    private LiquidityWalls BuildWalls(
    IReadOnlyList<DepthSnapshot> snapshots)
    {
        return new LiquidityWalls
        {
            Bid = _seriesBuilder.Build(
                snapshots,
                x => x.BidWallQuantity),

            Ask = _seriesBuilder.Build(
                snapshots,
                x => x.AskWallQuantity)
        };
    }

    private LiquidityConsumption BuildConsumption(
    IReadOnlyList<DepthSnapshot> snapshots)
    {
        return new LiquidityConsumption
        {
            Bid = _seriesBuilder.Build(
                snapshots,
                x => x.BidConsumption),

            Ask = _seriesBuilder.Build(
                snapshots,
                x => x.AskConsumption)
        };
    }

    private MetricState BuildSpread(
    IReadOnlyList<DepthSnapshot> snapshots)
    {
        return _seriesBuilder.Build(
            snapshots,
            x => x.Spread);
    }

    private MetricState BuildImbalance(
     IReadOnlyList<DepthSnapshot> snapshots)
    {
        return _seriesBuilder.Build(
            snapshots,
            x => x.Imbalance);
    }
}