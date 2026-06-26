using KinetixFlowEngine.Core.Domain.FundingRate;
using KinetixFlowEngine.Core.Domain.Liquidity;
using KinetixFlowEngine.Core.Domain.OI;
using KinetixFlowEngine.Core.Domain.Pricing;
using KinetixFlowEngine.Core.Domain.Trading;

namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketBuildRequest
{
    //-------------------------------------
    // Metadata
    //-------------------------------------

    public required DateTime TimestampUtc { get; init; }

    public required int Sequence { get; init; }

    public required int EngineBuild { get; init; }

    public required MarketTimeframe Timeframe { get; init; }

    public required MarketMode Mode { get; init; }

    public required DataFreshness Freshness { get; init; }

    //-------------------------------------
    // Completed Domains
    //-------------------------------------

    public required Price Price { get; init; }

    public required Trade Trade { get; init; }

    public required Liquidity.Depth Depth { get; init; }

    public required Funding Funding { get; init; }

    public required OpenInterest OpenInterest { get; init; }
}