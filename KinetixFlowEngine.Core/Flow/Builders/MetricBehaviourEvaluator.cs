using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Flow.Builders;

public static class MetricBehaviourEvaluator
{
    public static MetricBehaviour EvaluateLiquidity(
        MetricState metric)
    {
        return Evaluate(metric, 3m, 15m);
    }

    public static MetricBehaviour EvaluateImbalance(
        MetricState metric)
    {
        return Evaluate(metric, 2m, 10m);
    }

    public static MetricBehaviour EvaluatePressure(
        MetricState metric)
    {
        return Evaluate(metric, 2m, 12m);
    }

    private static MetricBehaviour Evaluate(
        MetricState metric,
        decimal stableThreshold,
        decimal strongThreshold)
    {
        var pct = metric.ChangePct;

        if (Math.Abs(pct) <= stableThreshold)
            return MetricBehaviour.Stable;

        if (pct >= strongThreshold)
            return MetricBehaviour.Expanding;

        if (pct > 0)
            return MetricBehaviour.Increasing;

        if (pct <= -strongThreshold)
            return MetricBehaviour.Decreasing;

        return MetricBehaviour.Decreasing;
    }
}