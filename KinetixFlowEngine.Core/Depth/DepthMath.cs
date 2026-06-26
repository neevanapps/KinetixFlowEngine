using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Depth;

public static class DepthMath
{
    /// <summary>
    /// Returns order book imbalance.
    /// Range:
    /// -100 = fully ask dominated
    /// 0    = balanced
    /// +100 = fully bid dominated
    /// </summary>
    public static decimal CalculateImbalance(
        decimal bidLiquidity,
        decimal askLiquidity)
    {
        var total = bidLiquidity + askLiquidity;

        if (total <= 0)
            return 0;

        return (bidLiquidity - askLiquidity)
             / total
             * 100m;
    }

    public static decimal Percentage(
        decimal value,
        decimal total)
    {
        if (total <= 0)
            return 0;

        return value / total * 100m;
    }

    public static decimal Average(
        IEnumerable<decimal> values)
    {
        var list = values.ToList();

        if (list.Count == 0)
            return 0;

        return list.Average();
    }

    public static MetricBehaviour GetBehaviour(
    MetricState metric)
    {
        var change = metric.ChangePct;

        if (Math.Abs(change) < 2m)
            return MetricBehaviour.Stable;

        if (change > 15m)
            return MetricBehaviour.Expanding;

        if (change > 2m)
            return MetricBehaviour.Increasing;

        if (change < -15m)
            return MetricBehaviour.Stable;

        return MetricBehaviour.Decreasing;
    }

    public static MetricState BuildMetric(
    IReadOnlyList<decimal> values)
    {
        if (values.Count == 0)
            return new MetricState();

        return new MetricState
        {
            Open = values.First(),
            High = values.Max(),
            Low = values.Min(),
            Close = values.Last()
        };
    }
}