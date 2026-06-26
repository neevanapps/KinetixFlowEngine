using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Common;

public static class MetricStateAggregator
{
    public static MetricState Aggregate(
        IEnumerable<MetricState> metrics)
    {
        var list = metrics.ToList();

        if (list.Count == 0)
            return new MetricState();

        return new MetricState
        {
            Open = list.First().Open,

            High = list.Max(x => x.High),

            Low = list.Min(x => x.Low),

            Close = list.Last().Close,

            Average = list.Average(x => x.Average),

            StdDev = list.Average(x => x.StdDev),

            SampleCount = list.Sum(x => x.SampleCount),

            Behaviour = list.Last().Behaviour
        };
    }
}