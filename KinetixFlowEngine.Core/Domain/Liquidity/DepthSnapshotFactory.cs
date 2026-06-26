using KinetixFlowEngine.Core.Domain.Liquidity;

namespace KinetixFlowEngine.Core.Domain.Liquidity;

public sealed class DepthSnapshotFactory
{
    public DepthSnapshot Create(
        DateTime timestampUtc,
        IReadOnlyList<DepthLevel> bids,
        IReadOnlyList<DepthLevel> asks)
    {
        bids ??= Array.Empty<DepthLevel>();
        asks ??= Array.Empty<DepthLevel>();

        var totalBidLiquidity = bids.Sum(x => x.Quantity);
        var totalAskLiquidity = asks.Sum(x => x.Quantity);

        var top5Bid = bids.Take(5).Sum(x => x.Quantity);
        var top5Ask = asks.Take(5).Sum(x => x.Quantity);

        var top10Bid = bids.Take(10).Sum(x => x.Quantity);
        var top10Ask = asks.Take(10).Sum(x => x.Quantity);

        var bidWall = bids
            .OrderByDescending(x => x.Quantity)
            .FirstOrDefault();

        var askWall = asks
            .OrderByDescending(x => x.Quantity)
            .FirstOrDefault();

        var bestBid = bids.Count == 0
            ? 0m
            : bids.Max(x => x.Price);

        var bestAsk = asks.Count == 0
            ? 0m
            : asks.Min(x => x.Price);

        var spread =
            bestBid == 0 || bestAsk == 0
                ? 0m
                : bestAsk - bestBid;

        var totalLiquidity =
            totalBidLiquidity + totalAskLiquidity;

        var imbalance =
            totalLiquidity == 0
                ? 0m
                : ((totalBidLiquidity - totalAskLiquidity)
                    / totalLiquidity) * 100m;

        return new DepthSnapshot
        {
            TimestampUtc = timestampUtc,

            Bids = bids,
            Asks = asks,

            TotalBidLiquidity = totalBidLiquidity,
            TotalAskLiquidity = totalAskLiquidity,

            BidWallQuantity = bidWall?.Quantity ?? 0m,
            AskWallQuantity = askWall?.Quantity ?? 0m,

            BidWallPrice = bidWall?.Price ?? 0m,
            AskWallPrice = askWall?.Price ?? 0m,

            Spread = spread,
            Imbalance = imbalance,

            BidConsumption = 0m,
            AskConsumption = 0m
        };
    }
}