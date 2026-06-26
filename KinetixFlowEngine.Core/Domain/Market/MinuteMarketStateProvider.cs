using KinetixFlowEngine.Core.Domain.FundingRate;
using KinetixFlowEngine.Core.Domain.Liquidity;
using KinetixFlowEngine.Core.Domain.OI;
using KinetixFlowEngine.Core.Domain.Pricing;
using KinetixFlowEngine.Core.Domain.Trading;

namespace KinetixFlowEngine.Core.Domain.Market;

public interface IMinuteMarketStateProvider
{
    Task<MarketState> CreateAsync(
        MinuteMarketContext context,
        CancellationToken cancellationToken = default);
}

public sealed class MinuteMarketStateProvider : IMinuteMarketStateProvider
{
    private readonly MarketBuildRequestFactory _requestFactory;
    private readonly IMarketStatePipeline _pipeline;

    public MinuteMarketStateProvider(
        MarketBuildRequestFactory requestFactory,
        IMarketStatePipeline pipeline)
    {
        _requestFactory = requestFactory;
        _pipeline = pipeline;
    }

    public async Task<MarketState> CreateAsync(
        MinuteMarketContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        cancellationToken.ThrowIfCancellationRequested();

        var request = _requestFactory.Create(
            context.TimestampUtc,
            context.Sequence,
            context.EngineBuild,
            context.Timeframe,
            context.Mode,
            context.Freshness,
            context.Price,
            context.Trade,
            context.Depth,
            context.Funding,
            context.OpenInterest);

        return await _pipeline.CreateAsync(
            request,
            cancellationToken);
    }
}