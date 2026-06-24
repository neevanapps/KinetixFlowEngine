using KinetixFlowEngine.Core.Database;

namespace KinetixFlowEngine.Core.Gpt.Models;

public sealed class HistoricalSnapshotSummary
{
    public int Sequence { get; init; }

    public decimal Price { get; init; }

    public string TrendLevel1 { get; init; } = "";
    public string TrendLevel2 { get; init; } = "";
    public string TrendLevel3 { get; init; } = "";

    public double ScoreZLevel1 { get; init; }
    public double ScoreZLevel2 { get; init; }
    public double ScoreZLevel3 { get; init; }

    public double MomentumLevel1 { get; init; }
    public double MomentumLevel2 { get; init; }
    public double MomentumLevel3 { get; init; }

    public double PersistenceLevel1 { get; init; }
    public double PersistenceLevel2 { get; init; }
    public double PersistenceLevel3 { get; init; }
}

public static class HistoricalSnapshotMapper
{
    public static HistoricalSnapshotSummary Map(MarketSnapshotEntity s)
    {
        return new HistoricalSnapshotSummary
        {
            Sequence = s.Sequence,

            Price = s.Price,

            TrendLevel1 = s.Trend10m,
            TrendLevel2 = s.Trend30m,
            TrendLevel3 = s.Trend60m,

            ScoreZLevel1 = s.ScoreZ10m,
            ScoreZLevel2 = s.ScoreZ30m,
            ScoreZLevel3 = s.ScoreZ60m,

            MomentumLevel1 = s.Momentum10m,
            MomentumLevel2 = s.Momentum30m,
            MomentumLevel3 = s.Momentum60m,

            PersistenceLevel1 = s.Persistence10m,
            PersistenceLevel2 = s.Persistence30m,
            PersistenceLevel3 = s.Persistence60m
        };
    }
}