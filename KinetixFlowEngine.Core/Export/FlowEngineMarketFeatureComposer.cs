using System.Text.Json;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Depth;
using KinetixFlowEngine.Core.Engine;

namespace KinetixFlowEngine.Core.Export;

public sealed class FlowEngineMarketFeatureComposer
{
    private const string Symbol = "BTCUSDT";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public FlowEngineMarketFeatureExport Compose(
        KinetixEngineResult result,
        DepthMinuteFeature depthMinute,
        DepthSnapshot? depthSnapshot)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(depthMinute);

        var price = (decimal)result.Price;
        var vwap = (decimal)result.VWAP;

        decimal? distanceFromVwapPct = vwap == 0
            ? null
            : (price - vwap) / vwap * 100m;

        var (
            bestBid,
            bestAsk,
            topBidQty,
            topAskQty,
            top10BidQty,
            top10AskQty,
            spreadBps
        ) = CalculateLiquidity(depthSnapshot);

        var export = new FlowEngineMarketFeatureExport
        {
            Symbol = Symbol,

            TimestampUtc = ToUtcOffset(depthMinute.TimestampUtc),
            CreatedUtc = DateTimeOffset.UtcNow,

            Price = price,
            PriceChange1m = depthMinute.PriceChange1m,

            Vwap = vwap,
            DistanceFromVwapPct = distanceFromVwapPct,
            Atr15m = result.ATR15m,

            ScoreZ = result.ScoreZ,
            VelocityZ = result.VelocityZ,
            ImbalanceZ = result.ImbalanceZ,
            CompressionZ = result.CompressionZ,
            ExhaustionZ = result.ExhaustionZ,

            Momentum = result.Momentum,
            Acceleration = result.Acceleration,
            Persistence = result.Persistence,
            NetPressure = result.NetPressure,
            FlowImpactEfficiency = result.FlowImpactEfficiency,

            AvgDepthImbalanceTop10 = depthMinute.AvgImbalance10,
            BullishBookPercent = depthMinute.BullishPercent,
            BearishBookPercent = depthMinute.BearishPercent,

            BullishPersistenceSeconds = depthMinute.BullishPersistenceSeconds,
            BearishPersistenceSeconds = depthMinute.BearishPersistenceSeconds,

            LargestBidWallAgeSec = depthMinute.LargestBidWallAgeSec,
            LargestAskWallAgeSec = depthMinute.LargestAskWallAgeSec,

            LargestBidWallQty = depthMinute.LargestBidWallQty,
            LargestAskWallQty = depthMinute.LargestAskWallQty,

            ConsumedBidWallCount = depthMinute.ConsumedBidWallCount,
            ConsumedAskWallCount = depthMinute.ConsumedAskWallCount,

            AvgBidQuantityChangePct = depthMinute.AvgBidQuantityChangePct,
            AvgAskQuantityChangePct = depthMinute.AvgAskQuantityChangePct,

            SpreadBps = spreadBps,
            BestBidPrice = bestBid,
            BestAskPrice = bestAsk,
            TopBidQty = topBidQty,
            TopAskQty = topAskQty,
            Top10BidQty = top10BidQty,
            Top10AskQty = top10AskQty
        };

        export.RawJson = JsonSerializer.Serialize(export, JsonOptions);

        return export;
    }

    private static DateTimeOffset ToUtcOffset(DateTime timestampUtc)
    {
        if (timestampUtc.Kind == DateTimeKind.Utc)
            return new DateTimeOffset(timestampUtc);

        return new DateTimeOffset(
            DateTime.SpecifyKind(timestampUtc, DateTimeKind.Utc));
    }

    private static (
        decimal? bestBid,
        decimal? bestAsk,
        decimal? topBidQty,
        decimal? topAskQty,
        decimal? top10BidQty,
        decimal? top10AskQty,
        double? spreadBps
    ) CalculateLiquidity(DepthSnapshot? snapshot)
    {
        if (snapshot?.Bids == null ||
            snapshot.Asks == null ||
            snapshot.Bids.Count == 0 ||
            snapshot.Asks.Count == 0)
        {
            return (null, null, null, null, null, null, null);
        }

        // If your DepthSnapshot already guarantees sorted order, this is still safe.
        // Since this runs once per minute, the small sort cost is acceptable.
        var bids = snapshot.Bids
            .OrderByDescending(x => x.Price)
            .Take(10)
            .ToList();

        var asks = snapshot.Asks
            .OrderBy(x => x.Price)
            .Take(10)
            .ToList();

        if (bids.Count == 0 || asks.Count == 0)
            return (null, null, null, null, null, null, null);

        var bestBid = bids[0].Price;
        var bestAsk = asks[0].Price;

        var topBidQty = bids[0].Quantity;
        var topAskQty = asks[0].Quantity;

        var top10BidQty = bids.Sum(x => x.Quantity);
        var top10AskQty = asks.Sum(x => x.Quantity);

        var mid = (bestBid + bestAsk) / 2m;

        double? spreadBps = mid <= 0
            ? null
            : (double)((bestAsk - bestBid) / mid * 10000m);

        return (
            bestBid,
            bestAsk,
            topBidQty,
            topAskQty,
            top10BidQty,
            top10AskQty,
            spreadBps);
    }
}