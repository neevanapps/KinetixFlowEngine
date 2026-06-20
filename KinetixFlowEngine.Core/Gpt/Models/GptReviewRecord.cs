namespace KinetixFlowEngine.Core.Gpt.Models;

public sealed class GptReviewRecord
{
    public DateTime CreatedUtc { get; init; }

    public int Sequence { get; init; }

    public string ModelName { get; set; } = "";

    public GptMarketSnapshotV2 Snapshot { get; init; } = default!;

    public GptAssessment Assessment { get; init; } = default!;

    public string RawResponse { get; init; } = string.Empty;
}