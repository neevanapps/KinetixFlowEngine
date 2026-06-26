namespace KinetixFlowEngine.Core.Domain.Common;

public enum MetricBehaviour
{
    Unknown = 0,

    Stable,

    Increasing,

    StronglyIncreasing,

    Decreasing,

    StronglyDecreasing,

    Expanding,

    Contracting,

    Appearing,

    Disappearing
}

public enum MarketBias
{
    Unknown = 0,

    Bullish,

    Bearish,

    Neutral,

    Mixed
}

public enum MarketStrength
{
    Unknown = 0,

    Weak,

    Moderate,

    Strong,

    Extreme
}

public enum MarketEventType
{
    Unknown = 0,

    //-----------------------
    // Participation
    //-----------------------

    VolumeIncreasing,

    VolumeDecreasing,

    AggressiveBuying,

    AggressiveSelling,

    LargeBuyerAppeared,

    LargeSellerAppeared,

    BuyerExhaustion,

    SellerExhaustion,

    //-----------------------
    // Liquidity
    //-----------------------

    LiquidityAdded,

    LiquidityRemoved,

    BidWallBuilt,

    AskWallBuilt,

    BidWallConsumed,

    AskWallConsumed,

    SpreadExpanded,

    SpreadContracted,

    //-----------------------
    // Price
    //-----------------------

    Breakout,

    Breakdown,

    FakeBreakout,

    LiquiditySweep,

    VWAPReclaimed,

    VWAPLost,

    //-----------------------
    // Positioning
    //-----------------------

    FundingSpike,

    FundingDrop,

    OpenInterestIncreasing,

    OpenInterestDecreasing,

    LongBuildUp,

    ShortBuildUp,

    LongLiquidation,

    ShortCovering
}