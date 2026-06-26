namespace KinetixFlowEngine.Core.Domain.Market;

public sealed class MarketBuildRequestFactory
{
    public MarketBuildRequest Create(
        DateTime timestampUtc,
        int sequence,
        int engineBuild,
        MarketTimeframe timeframe,
        MarketMode mode,
        DataFreshness freshness,
        Pricing.Price price,
        Trading.Trade trade,
        Liquidity.Depth depth,
        FundingRate.Funding funding,
        OI.OpenInterest openInterest)
    {
        return new MarketBuildRequest
        {
            TimestampUtc = timestampUtc,

            Sequence = sequence,

            EngineBuild = engineBuild,

            Timeframe = timeframe,

            Mode = mode,

            Freshness = freshness,

            Price = price,

            Trade = trade,

            Depth = depth,

            Funding = funding,

            OpenInterest = openInterest
        };
    }
}