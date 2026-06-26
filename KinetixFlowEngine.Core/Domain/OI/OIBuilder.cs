using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.OI;

public sealed class OIBuilder
{
    private decimal? _previousValue;

    public OpenInterest Build(OpenInterestObservation observation)
    {
        decimal change = 0;

        if (_previousValue.HasValue)
            change = observation.Value - _previousValue.Value;

        _previousValue = observation.Value;

        return new OpenInterest
        {
            Value = MetricStateFactory.Create(observation.Value),

            Change = MetricStateFactory.Create(change)
        };
    }
}