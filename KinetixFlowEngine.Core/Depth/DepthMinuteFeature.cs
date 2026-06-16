namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthMinuteFeature
{
    public DateTime TimestampUtc { get; set; }

    public decimal Price { get; set; }

    public decimal PriceChange1m { get; set; }

    public double AvgImbalance10 { get; set; }

    public double BullishPercent { get; set; }

    public double BearishPercent { get; set; }

    public double BullishPersistenceSeconds { get; set; }

    public double BearishPersistenceSeconds { get; set; }

    public double LargestBidWallAgeSec { get; set; }

    public double LargestAskWallAgeSec { get; set; }

    public decimal LargestBidWallQty { get; set; }

    public decimal LargestAskWallQty { get; set; }

    public int ConsumedBidWallCount { get; set; }

    public int ConsumedAskWallCount { get; set; }

    public double AvgBidQuantityChangePct { get; set; }

    public double AvgAskQuantityChangePct { get; set; }
}