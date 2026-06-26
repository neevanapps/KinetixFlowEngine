using KinetixFlowEngine.Core.Domain.Common;

public static class MetricStateFactory
{
    public static MetricState Create(decimal value)
    {
        return new MetricState
        {
            Open = value,
            High = value,
            Low = value,
            Close = value,
            Average = value,
            SampleCount = 1
        };
    }

    public static MetricState Create(int value)
    {
        return Create((decimal)value);
    }
}