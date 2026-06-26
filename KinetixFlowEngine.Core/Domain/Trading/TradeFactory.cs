using KinetixFlowEngine.Core.Domain.Common;
using KinetixFlowEngine.Core.Domain.Market;

namespace KinetixFlowEngine.Core.Domain.Trading;

public sealed class TradeFactory : DomainFactory<Trade>
{
    private readonly TradeBuilder _builder;
    private readonly TradeSummaryBuilder _summaryBuilder;
    private readonly TradeEventBuilder _eventBuilder;

    public TradeFactory(
        TradeBuilder builder,
        TradeSummaryBuilder summaryBuilder,
        TradeEventBuilder eventBuilder)
    {
        _builder = builder;
        _summaryBuilder = summaryBuilder;
        _eventBuilder = eventBuilder;
    }

    public Trade Build(TradeMinuteSnapshot snapshot)
    {
        var trade = _builder.Build(snapshot);

        return Complete(
            trade,
            _summaryBuilder.Build,
            _eventBuilder.Build);
    }

    public Trade Aggregate(
    MarketAggregationContext context)
    {
        return new Trade
        {
            Volume = new TradeVolume
            {
                Buy = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.Volume.Buy)),

                Sell = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.Volume.Sell)),

                Delta = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.Volume.Delta))
            },

            Trades = new TradeCount
            {
                Buy = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.Trades.Buy)),

                Sell = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.Trades.Sell))
            },

            TradeSize = new TradeSize
            {
                Buy = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.TradeSize.Buy)),

                Sell = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.TradeSize.Sell)),

                LargestBuy = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.TradeSize.LargestBuy)),

                LargestSell = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.TradeSize.LargestSell))
            },

            Execution = new TradeExecution
            {
                BuyVWAP = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.Execution.BuyVWAP)),

                SellVWAP = MetricStateAggregator.Aggregate(
                    context.States.Select(x => x.Trade.Execution.SellVWAP))
            },

            Summary = context.Last.Trade.Summary,

            Events = context
                .States
                .SelectMany(x => x.Trade.Events)
                .ToList()
        };
    }
}