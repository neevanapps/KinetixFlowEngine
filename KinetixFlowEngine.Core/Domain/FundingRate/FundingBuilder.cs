using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.FundingRate;

public sealed class FundingBuilder
{
    public Funding Build(
        FundingObservation observation)
    {
        return new Funding
        {
            Rate = MetricStateFactory.Create(
                observation.Rate),

            Pressure = MetricStateFactory.Create(
                CalculatePressure(observation.Rate))
        };
    }

    /// <summary>
    /// Converts funding rate into a simple pressure score.
    /// Range: 0 - 100
    /// </summary>
    private static decimal CalculatePressure(
        decimal fundingRate)
    {
        var pressure = Math.Abs(fundingRate) * 100000m;

        return Math.Min(pressure, 100m);
    }
}