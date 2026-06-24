namespace KinetixFlowEngine.Core.Gpt.Models;

public sealed class SmoothedModelReview
{
    public string ModelName { get; init; } = "";

    public double Score { get; init; }

    public double LongConfidence { get; init; }

    public double ShortConfidence { get; init; }

    public string Direction { get; init; } = "";

    public double ScoreVelocity { get; init; }

    public int LatestSequence { get; init; }
}