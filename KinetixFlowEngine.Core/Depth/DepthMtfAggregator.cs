using KinetixFlowEngine.Core.Data;

namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthMtfAggregator
{

    public DepthMtfSnapshot Build()
    {
        //_logger.LogInformation("Building DepthMtfSnapshot with {RowCount} rows", _rows.Count);
        return new DepthMtfSnapshot
        {
            //Imbalance =
            //[
            //    Avg(KinetixConstants.Level1, x => x.AvgImbalance10),
            //    Avg(KinetixConstants.Level2, x => x.AvgImbalance10),
            //    Avg(KinetixConstants.Level3, x => x.AvgImbalance10)
            //],

            //BullishPercent =
            //[
            //    Avg(KinetixConstants.Level1, x => x.BullishPercent),
            //    Avg(KinetixConstants.Level2, x => x.BullishPercent),
            //    Avg(KinetixConstants.Level3, x => x.BullishPercent)
            //],

            //BullishPersistence =
            //[
            //    Avg(KinetixConstants.Level1, x => x.BullishPersistenceSeconds),
            //    Avg(KinetixConstants.Level2, x => x.BullishPersistenceSeconds),
            //    Avg(KinetixConstants.Level3, x => x.BullishPersistenceSeconds)
            //],

            //BidWallAge =
            //[
            //    Avg(KinetixConstants.Level1, x => x.LargestBidWallAgeSec),
            //    Avg(KinetixConstants.Level2, x => x.LargestBidWallAgeSec),
            //    Avg(KinetixConstants.Level3, x => x.LargestBidWallAgeSec)
            //],

            //AskWallAge =
            //[
            //    Avg(KinetixConstants.Level1, x => x.LargestAskWallAgeSec),
            //    Avg(KinetixConstants.Level2, x => x.LargestAskWallAgeSec),
            //    Avg(KinetixConstants.Level3, x => x.LargestAskWallAgeSec)
            //],

            //BidWallQty =
            //[
            //    Avg(KinetixConstants.Level1, x => (double)x.LargestBidWallQty),
            //    Avg(KinetixConstants.Level2, x => (double)x.LargestBidWallQty),
            //    Avg(KinetixConstants.Level3, x => (double)x.LargestBidWallQty)
            //],

            //AskWallQty =
            //[
            //    Avg(KinetixConstants.Level1, x => (double)x.LargestAskWallQty),
            //    Avg(KinetixConstants.Level2, x => (double)x.LargestAskWallQty),
            //    Avg(KinetixConstants.Level3, x => (double)x.LargestAskWallQty)
            //],

            //BidConsumption =
            //[
            //    Avg(KinetixConstants.Level1, x => x.ConsumedBidWallCount),
            //    Avg(KinetixConstants.Level2, x => x.ConsumedBidWallCount),
            //    Avg(KinetixConstants.Level3, x => x.ConsumedBidWallCount)
            //],

            //AskConsumption =
            //[
            //    Avg(KinetixConstants.Level1, x => x.ConsumedAskWallCount),
            //    Avg(KinetixConstants.Level2, x => x.ConsumedAskWallCount),
            //    Avg(KinetixConstants.Level3, x => x.ConsumedAskWallCount)
            //]
        };
    }

    //private double Avg(int lookback, Func<DepthMinuteState, double> selector)
    //{
    //    var rows = _rows.TakeLast(lookback).ToList();

    //    if (rows.Count == 0)
    //        return 0;

    //    return rows.Average(selector);
    //}
}