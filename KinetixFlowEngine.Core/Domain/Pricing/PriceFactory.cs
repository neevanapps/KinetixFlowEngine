using KinetixFlowEngine.Core.Domain.Common;
using KinetixFlowEngine.Core.Domain.Market;

namespace KinetixFlowEngine.Core.Domain.Pricing;

public sealed class PriceFactory : DomainFactory<Price>
{
    private readonly PriceBuilder _builder;

    private readonly PriceSummaryBuilder _summaryBuilder;

    private readonly PriceEventBuilder _eventBuilder;

    public PriceFactory(
        PriceBuilder builder,
        PriceSummaryBuilder summaryBuilder,
        PriceEventBuilder eventBuilder)
    {
        _builder = builder;
        _summaryBuilder = summaryBuilder;
        _eventBuilder = eventBuilder;
    }

    public Price Build(
        MinuteCandle candle,
        decimal vwap,
        decimal atr,
        decimal previousClose)
    {
        var price = _builder.Build(
            candle,
            vwap,
            atr,
            previousClose);

        return Complete(
            price,
            _summaryBuilder.Build,
            _eventBuilder.Build);
    }

    public Price Aggregate(
    MarketAggregationContext context)
    {
        return new Price
        {
            Candle = CandleAggregator.Aggregate(
                context.States.Select(x => x.Price.Candle)),

            VWAP = context.States.Average(x => x.Price.VWAP),

            ATR = context.States.Average(x => x.Price.ATR),

            DistanceFromVWAP =
                MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Price.DistanceFromVWAP)),

            DistanceFromPreviousClose =
                MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Price.DistanceFromPreviousClose)),

            Summary = context.Last.Price.Summary,

            Events = context.States
                .SelectMany(x => x.Price.Events)
                .ToList()
        };
    }
}