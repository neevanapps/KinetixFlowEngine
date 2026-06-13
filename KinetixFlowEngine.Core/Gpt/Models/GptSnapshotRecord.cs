using KinetixFlowEngine.Core.Gpt.Models;

public sealed class GptSnapshotRecord
{
    public DateTime CreatedUtc { get; init; }

    public GptMarketSnapshot Snapshot { get; init; } = default!;
}