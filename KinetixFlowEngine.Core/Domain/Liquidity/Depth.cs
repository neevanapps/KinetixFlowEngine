using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Liquidity
{
    public sealed class Depth : IMarketDomain
    {
        public LiquidityVolume Liquidity { get; set; } = new();

        public LiquidityWalls Walls { get; set; } = new();

        public LiquidityConsumption Consumption { get; set; } = new();

        public MetricState Spread { get; set; } = new();

        public MetricState Imbalance { get; set; } = new();

        public DomainSummary Summary { get; set; } = new();

        public MetricState Pressure { get; set; } = new();

        public IReadOnlyList<MarketEvent> Events { get; set; }
            = Array.Empty<MarketEvent>();
    }

    public sealed class LiquidityConsumption
    {
        public MetricState Bid { get; set; } = new();

        public MetricState Ask { get; set; } = new();


    }

    public sealed class LiquidityWalls
    {
        public MetricState Bid { get; set; } = new();

        public MetricState Ask { get; set; } = new();
    }

    public sealed class LiquidityVolume
    {
        public MetricState Bid { get; set; } = new();

        public MetricState Ask { get; set; } = new();
    }

    public sealed class DepthSnapshot
    {
        public DateTime TimestampUtc { get; init; }

        public IReadOnlyList<DepthLevel> Bids { get; init; }
            = Array.Empty<DepthLevel>();

        public IReadOnlyList<DepthLevel> Asks { get; init; }
            = Array.Empty<DepthLevel>();

        //-------------------------------------
        // Derived Metrics
        //-------------------------------------

        public decimal TotalBidLiquidity { get; init; }

        public decimal TotalAskLiquidity { get; init; }

        public decimal BidWallQuantity { get; init; }

        public decimal AskWallQuantity { get; init; }

        public decimal BidWallPrice { get; init; }

        public decimal AskWallPrice { get; init; }

        public decimal Spread { get; init; }

        public decimal Imbalance { get; init; }

        public decimal BidConsumption { get; init; }

        public decimal AskConsumption { get; init; }
    }

    public sealed class DepthLevel
    {
        public decimal Price { get; init; }

        public decimal Quantity { get; init; }
    }

    public sealed class DepthWallState
    {
        public decimal BidWallPrice { get; init; }

        public decimal BidWallQuantity { get; init; }

        public decimal AskWallPrice { get; init; }

        public decimal AskWallQuantity { get; init; }

        public int BidWallAgeSeconds { get; init; }

        public int AskWallAgeSeconds { get; init; }

        public bool BidWallConsumed { get; init; }

        public bool AskWallConsumed { get; init; }

        public MetricBehaviour BidBehaviour { get; init; }

        public MetricBehaviour AskBehaviour { get; init; }
    }
}
