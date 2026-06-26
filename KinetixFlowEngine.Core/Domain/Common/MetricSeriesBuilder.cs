using System;
using System.Collections.Generic;
using System.Linq;

namespace KinetixFlowEngine.Core.Domain.Common;

public sealed class MetricSeriesBuilder
{
    /// <summary>
    /// Builds a MetricState from a decimal series.
    /// </summary>
    public MetricState Build(IEnumerable<decimal> values)
    {
        var series = values.ToArray();

        if (series.Length == 0)
            return new MetricState();

        var average = series.Average();

        var variance = series.Length == 1
            ? 0m
            : series.Sum(x => (x - average) * (x - average)) / series.Length;

        return new MetricState
        {
            Open = series.First(),

            High = series.Max(),

            Low = series.Min(),

            Close = series.Last(),

            Average = average,

            StdDev = (decimal)Math.Sqrt((double)variance),

            SampleCount = series.Length,

            Behaviour = GetBehaviour(series)
        };
    }

    /// <summary>
    /// Builds a MetricState from any collection using a selector.
    /// </summary>
    public MetricState Build<T>(
        IEnumerable<T> source,
        Func<T, decimal> selector)
    {
        return Build(source.Select(selector));
    }

    /// <summary>
    /// Determines the overall behaviour of the series.
    /// </summary>
    private static MetricBehaviour GetBehaviour(decimal[] series)
    {
        if (series.Length < 2)
            return MetricBehaviour.Stable;

        var first = series.First();
        var last = series.Last();

        if (first == 0m)
        {
            if (last > 0m)
                return MetricBehaviour.Appearing;

            return MetricBehaviour.Stable;
        }

        var changePct = ((last - first) / first) * 100m;

        if (changePct >= 30m)
            return MetricBehaviour.StronglyIncreasing;

        if (changePct >= 10m)
            return MetricBehaviour.Increasing;

        if (changePct <= -30m)
            return MetricBehaviour.StronglyDecreasing;

        if (changePct <= -10m)
            return MetricBehaviour.Decreasing;

        return MetricBehaviour.Stable;
    }
}