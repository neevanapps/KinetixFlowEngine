namespace KinetixFlowEngine.Core.Export;

public sealed record FlowEngineMarketFeatureExport
{
    public Guid FeatureId { get; init; } = Guid.NewGuid();
    public string Symbol { get; init; } = "BTCUSDT";
    public DateTimeOffset TimestampUtc { get; init; }
    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;

    public decimal Price { get; init; }
    public decimal PriceChange1m { get; init; }
    public decimal? Vwap { get; init; }
    public decimal? DistanceFromVwapPct { get; init; }
    public double? Atr15m { get; init; }

    public double ScoreZ { get; init; }
    public double VelocityZ { get; init; }
    public double ImbalanceZ { get; init; }
    public double CompressionZ { get; init; }
    public double ExhaustionZ { get; init; }
    public double Momentum { get; init; }
    public double Acceleration { get; init; }
    public double Persistence { get; init; }
    public double NetPressure { get; init; }
    public double FlowImpactEfficiency { get; init; }

    public double AvgDepthImbalanceTop10 { get; init; }
    public double BullishBookPercent { get; init; }
    public double BearishBookPercent { get; init; }
    public double BullishPersistenceSeconds { get; init; }
    public double BearishPersistenceSeconds { get; init; }
    public double LargestBidWallAgeSec { get; init; }
    public double LargestAskWallAgeSec { get; init; }
    public decimal LargestBidWallQty { get; init; }
    public decimal LargestAskWallQty { get; init; }
    public int ConsumedBidWallCount { get; init; }
    public int ConsumedAskWallCount { get; init; }
    public double AvgBidQuantityChangePct { get; init; }
    public double AvgAskQuantityChangePct { get; init; }

    public double? SpreadBps { get; init; }
    public decimal? BestBidPrice { get; init; }
    public decimal? BestAskPrice { get; init; }
    public decimal? TopBidQty { get; init; }
    public decimal? TopAskQty { get; init; }
    public decimal? Top10BidQty { get; init; }
    public decimal? Top10AskQty { get; init; }

    public string RawJson { get; set; } = "{}";
}
