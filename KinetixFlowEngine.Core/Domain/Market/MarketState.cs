using KinetixFlowEngine.Core.Domain.Common;
using KinetixFlowEngine.Core.Domain.FundingRate;
using KinetixFlowEngine.Core.Domain.Liquidity;
using KinetixFlowEngine.Core.Domain.OI;
using KinetixFlowEngine.Core.Domain.Pricing;
using KinetixFlowEngine.Core.Domain.Trading;
using System.Diagnostics;

namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketState
{
    //-----------------------------------------
    // Metadata
    //-----------------------------------------
    public Guid Id { get; set; }

    public int Sequence { get; set; }

    public int EngineBuild { get; set; }
    //-----------------------------------------
    // Identity
    //-----------------------------------------

    public DateTime TimestampUtc { get; set; }

    public MarketTimeframe Timeframe { get; set; }

    public DataFreshness Freshness { get; set; } = new();

    public byte QualityScore { get; set; }


    //-----------------------------------------
    // Domains
    //-----------------------------------------

    public Price Price { get; set; } = new();

    public Trade Trade { get; set; } = new();

    public Liquidity.Depth Depth { get; set; } = new();

    public Funding Funding { get; set; } = new();

    public OpenInterest OpenInterest { get; set; } = new();

    //----------------------------------------
    // Overall interpretation
    //----------------------------------------

    public MarketMode Mode { get; set; }

    public MarketRegime Regime { get; set; }

    public DomainSummary Summary { get; set; } = new();

    //----------------------------------------
    // Combined events
    //----------------------------------------

    public IReadOnlyList<MarketEvent> Events { get; set; }
        = Array.Empty<MarketEvent>();
}

public enum MarketMode
{
    Live,
    Replay,
    Bootstrap,
    Synthetic
}

public enum MarketRegime
{
    TrendingBull,
    TrendingBear,
    Range,
    Accumulation,
    Distribution,
    Expansion,
    Compression,
    Transition
}

public sealed class DataFreshness
{
    public TimeSpan TradeAge { get; init; }

    public TimeSpan DepthAge { get; init; }

    public TimeSpan FundingAge { get; init; }

    public TimeSpan OIAge { get; init; }
}

public enum MarketTimeframe
{
    OneMinute = 1,
    FiveMinutes = 5,
    TenMinutes = 10,
    FifteenMinutes = 15,
    ThirtyMinutes = 30,
    OneHour = 60,
    FourHours = 240,
    OneDay = 1440
}