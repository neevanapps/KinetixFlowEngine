using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Market;

public interface IMarketStateFactory
{
    MarketState Build(
        MarketBuildRequest request);
}

public sealed class MarketStateFactory : IMarketStateFactory
{
    private readonly MarketSummaryBuilder _summaryBuilder;

    private readonly MarketEventBuilder _eventBuilder;

    private readonly MarketRegimeBuilder _regimeBuilder;

    private readonly MarketQualityBuilder _qualityBuilder;

    public MarketStateFactory(
        MarketSummaryBuilder summaryBuilder,
        MarketEventBuilder eventBuilder,
        MarketRegimeBuilder regimeBuilder,
        MarketQualityBuilder qualityBuilder)
    {
        _summaryBuilder = summaryBuilder;
        _eventBuilder = eventBuilder;
        _regimeBuilder = regimeBuilder;
        _qualityBuilder = qualityBuilder;
    }

    public MarketState Build(
        MarketBuildRequest request)
    {
        var state = new MarketState
        {
            Id = Guid.NewGuid(),

            TimestampUtc = request.TimestampUtc,

            Sequence = request.Sequence,

            EngineBuild = request.EngineBuild,

            Timeframe = request.Timeframe,

            Mode = request.Mode,

            Freshness = request.Freshness,

            Price = request.Price,

            Trade = request.Trade,

            Depth = request.Depth,

            Funding = request.Funding,

            OpenInterest = request.OpenInterest
        };

        state.Regime =
            _regimeBuilder.Build(state);

        state.QualityScore =
            _qualityBuilder.Build(state);

        state.Summary =
            _summaryBuilder.Build(state);

        state.Events =
            _eventBuilder.Build(state);

        Validate(state);

        return state;
    }

    private static void Validate(
    MarketState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.TimestampUtc == default)
            throw new InvalidOperationException(
                "TimestampUtc is required.");

        if (state.Price is null)
            throw new InvalidOperationException(
                "Price is required.");

        if (state.Trade is null)
            throw new InvalidOperationException(
                "Trade is required.");

        if (state.Depth is null)
            throw new InvalidOperationException(
                "Depth is required.");

        if (state.Funding is null)
            throw new InvalidOperationException(
                "Funding is required.");

        if (state.OpenInterest is null)
            throw new InvalidOperationException(
                "OpenInterest is required.");
    }
}