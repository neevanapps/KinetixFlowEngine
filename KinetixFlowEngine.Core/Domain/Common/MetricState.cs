namespace KinetixFlowEngine.Core.Domain.Common;

public sealed class MetricState
{
    //----------------------------------------
    // OHLC
    //----------------------------------------

    public decimal Open { get; set; }

    public decimal High { get; set; }

    public decimal Low { get; set; }

    public decimal Close { get; set; }

    //----------------------------------------
    // Statistics
    //----------------------------------------

    /// <summary>
    /// Average value across the aggregation period.
    /// </summary>
    public decimal Average { get; set; }

    /// <summary>
    /// Number of observations used to build this metric.
    /// Useful for weighted aggregation.
    /// </summary>
    public int SampleCount { get; set; }

    public decimal StdDev { get; set; }

    //----------------------------------------
    // Derived Values
    //----------------------------------------

    public decimal Change => Close - Open;

    public decimal Range => High - Low;

    public decimal ChangePct =>
        Open == 0
            ? 0
            : (Close - Open) / Open * 100m;

    /// <summary>
    /// Alias for Close. Makes calling code easier to read.
    /// </summary>
    public decimal Latest => Close;

    //----------------------------------------
    // Behaviour
    //----------------------------------------

    public MetricBehaviour Behaviour { get; set; }
}