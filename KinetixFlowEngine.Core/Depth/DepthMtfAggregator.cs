namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthMtfAggregator
{
    private readonly IReadOnlyList<DepthMinuteFeature> _rows;

    private readonly int _level1 = 15;
    private readonly int _level2 = 45;
    private readonly int _level3 = 120;
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
                Avg(_level1, x => x.AvgImbalance10),
                Avg(_level2, x => x.AvgImbalance10),
                Avg(_level3, x => x.AvgImbalance10)
            ],

            BullishPercent =
            [
                Avg(_level1, x => x.BullishPercent),
                Avg(_level2, x => x.BullishPercent),
                Avg(_level3, x => x.BullishPercent)
            ],

            BullishPersistence =
            [
                Avg(_level1, x => x.BullishPersistenceSeconds),
                Avg(_level2, x => x.BullishPersistenceSeconds),
                Avg(_level3, x => x.BullishPersistenceSeconds)
            ],

            BidWallAge =
            [
                Avg(_level1, x => x.LargestBidWallAgeSec),
                Avg(_level2, x => x.LargestBidWallAgeSec),
                Avg(_level3, x => x.LargestBidWallAgeSec)
            ],

            AskWallAge =
            [
                Avg(_level1, x => x.LargestAskWallAgeSec),
                Avg(_level2, x => x.LargestAskWallAgeSec),
                Avg(_level3, x => x.LargestAskWallAgeSec)
            ],

            BidWallQty =
            [
                Avg(_level1, x => (double)x.LargestBidWallQty),
                Avg(_level2, x => (double)x.LargestBidWallQty),
                Avg(_level3, x => (double)x.LargestBidWallQty)
            ],

            AskWallQty =
            [
                Avg(_level1, x => (double)x.LargestAskWallQty),
                Avg(_level2, x => (double)x.LargestAskWallQty),
                Avg(_level3, x => (double)x.LargestAskWallQty)
            ],

            BidConsumption =
            [
                Avg(_level1, x => x.ConsumedBidWallCount),
                Avg(_level2, x => x.ConsumedBidWallCount),
                Avg(_level3, x => x.ConsumedBidWallCount)
            ],

            AskConsumption =
            [
                Avg(_level1, x => x.ConsumedAskWallCount),
                Avg(_level2, x => x.ConsumedAskWallCount),
                Avg(_level3, x => x.ConsumedAskWallCount)
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