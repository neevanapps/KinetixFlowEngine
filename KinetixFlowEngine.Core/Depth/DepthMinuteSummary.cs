namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthMinuteSummary
{
    public DateTime TimestampUtc { get; set; }

    public double AverageImbalanceTop5 { get; set; }

    public double AverageImbalanceTop10 { get; set; }

    public double MaxBullishImbalance { get; set; }

    public double MaxBearishImbalance { get; set; }

    public double BullishBookPercent { get; set; }

    public double BearishBookPercent { get; set; }

    public decimal PriceChange1m { get; set; }

    public int BullishPersistenceSeconds { get; set; }

    public int BearishPersistenceSeconds { get; set; }

    public int SampleCount { get; set; }
}