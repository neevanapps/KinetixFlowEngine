using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Flow.Builders;

public abstract class MinuteStateBuilderBase
{
    protected readonly MetricSeriesBuilder Builder1 = new();
    protected readonly MetricSeriesBuilder Builder2 = new();
    protected readonly MetricSeriesBuilder Builder3 = new();
    protected readonly MetricSeriesBuilder Builder4 = new();
    protected readonly MetricSeriesBuilder Builder5 = new();
    protected readonly MetricSeriesBuilder Builder6 = new();

    //protected void Reset()
    //{
    //    Builder1.Reset();
    //    Builder2.Reset();
    //    Builder3.Reset();
    //    Builder4.Reset();
    //    Builder5.Reset();
    //    Builder6.Reset();
    //}
}