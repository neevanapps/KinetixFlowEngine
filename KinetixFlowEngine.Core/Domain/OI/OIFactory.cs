using KinetixFlowEngine.Core.Domain.Common;
using KinetixFlowEngine.Core.Domain.Market;

namespace KinetixFlowEngine.Core.Domain.OI;

public sealed class OIFactory : DomainFactory<OpenInterest>
{
    private readonly OIBuilder _builder;
    private readonly OISummaryBuilder _summaryBuilder;
    private readonly OIEventBuilder _eventBuilder;

    public OIFactory(
        OIBuilder builder,
        OISummaryBuilder summaryBuilder,
        OIEventBuilder eventBuilder)
    {
        _builder = builder;
        _summaryBuilder = summaryBuilder;
        _eventBuilder = eventBuilder;
    }

    public OpenInterest Build(OpenInterestObservation observation)
    {
        var oi = _builder.Build(observation);

        return Complete(
            oi,
            _summaryBuilder.Build,
            _eventBuilder.Build);
    }

    public OpenInterest Aggregate(
    MarketAggregationContext context)
    {
        return new OpenInterest
        {
            Value = MetricStateAggregator.Aggregate(
                context.States.Select(x => x.OpenInterest.Value)),

            Change = MetricStateAggregator.Aggregate(
                context.States.Select(x => x.OpenInterest.Change)),

            Summary = context.Last.OpenInterest.Summary,

            Events = context
                .States
                .SelectMany(x => x.OpenInterest.Events)
                .ToList()
        };
    }
}