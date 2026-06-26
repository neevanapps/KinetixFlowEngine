using KinetixFlowEngine.Core.Domain.Common;
using KinetixFlowEngine.Core.Domain.Market;

namespace KinetixFlowEngine.Core.Domain.FundingRate;

public sealed class FundingFactory : DomainFactory<Funding>
{
    private readonly FundingBuilder _builder;

    private readonly FundingSummaryBuilder _summaryBuilder;

    private readonly FundingEventBuilder _eventBuilder;

    public FundingFactory(
        FundingBuilder builder,
        FundingSummaryBuilder summaryBuilder,
        FundingEventBuilder eventBuilder)
    {
        _builder = builder;
        _summaryBuilder = summaryBuilder;
        _eventBuilder = eventBuilder;
    }

    public Funding Build(
        FundingObservation observation)
    {
        var funding =
            _builder.Build(observation);

        return Complete(
            funding,
            _summaryBuilder.Build,
            _eventBuilder.Build);
    }

    public Funding Aggregate(
    MarketAggregationContext context)
    {
        return new Funding
        {
            Rate = MetricStateAggregator.Aggregate(
                context.States.Select(x => x.Funding.Rate)),

            Pressure = MetricStateAggregator.Aggregate(
                context.States.Select(x => x.Funding.Pressure)),

            Summary = context.Last.Funding.Summary,

            Events = context
                .States
                .SelectMany(x => x.Funding.Events)
                .ToList()
        };
    }
}