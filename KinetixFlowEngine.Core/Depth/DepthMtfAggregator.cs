namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthMtfAggregator
{
    private readonly IReadOnlyList<DepthMinuteFeature> _rows;

    public DepthMtfAggregator(
        IReadOnlyList<DepthMinuteFeature> rows)
    {
        _rows = rows;
    }

    public DepthMtfSnapshot Build()
    {
        return new DepthMtfSnapshot
        {
            Imbalance =
            [
                Avg(10, x => x.AvgImbalance10),
                Avg(30, x => x.AvgImbalance10),
                Avg(60, x => x.AvgImbalance10)
            ],

            BullishPercent =
            [
                Avg(10, x => x.BullishPercent),
                Avg(30, x => x.BullishPercent),
                Avg(60, x => x.BullishPercent)
            ],

            BullishPersistence =
            [
                Avg(10, x => x.BullishPersistenceSeconds),
                Avg(30, x => x.BullishPersistenceSeconds),
                Avg(60, x => x.BullishPersistenceSeconds)
            ],

            BidWallAge =
            [
                Avg(10, x => x.LargestBidWallAgeSec),
                Avg(30, x => x.LargestBidWallAgeSec),
                Avg(60, x => x.LargestBidWallAgeSec)
            ],

            AskWallAge =
            [
                Avg(10, x => x.LargestAskWallAgeSec),
                Avg(30, x => x.LargestAskWallAgeSec),
                Avg(60, x => x.LargestAskWallAgeSec)
            ],

            BidWallQty =
            [
                Avg(10, x => (double)x.LargestBidWallQty),
                Avg(30, x => (double)x.LargestBidWallQty),
                Avg(60, x => (double)x.LargestBidWallQty)
            ],

            AskWallQty =
            [
                Avg(10, x => (double)x.LargestAskWallQty),
                Avg(30, x => (double)x.LargestAskWallQty),
                Avg(60, x => (double)x.LargestAskWallQty)
            ],

            BidConsumption =
            [
                Avg(10, x => x.ConsumedBidWallCount),
                Avg(30, x => x.ConsumedBidWallCount),
                Avg(60, x => x.ConsumedBidWallCount)
            ],

            AskConsumption =
            [
                Avg(10, x => x.ConsumedAskWallCount),
                Avg(30, x => x.ConsumedAskWallCount),
                Avg(60, x => x.ConsumedAskWallCount)
            ]
        };
    }

    private double Avg(
        int lookback,
        Func<DepthMinuteFeature, double> selector)
    {
        var rows =
            _rows.TakeLast(lookback).ToList();

        if (rows.Count == 0)
            return 0;

        return rows.Average(selector);
    }
}