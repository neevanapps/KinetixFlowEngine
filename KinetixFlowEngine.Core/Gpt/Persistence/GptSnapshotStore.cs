using System.Text;
using System.Text.Json;
using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Persistence;

public sealed class GptSnapshotStore
{
    private readonly string _filePath;

    public GptSnapshotStore()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "persist", "gpt");
        Directory.CreateDirectory(folder);

        _filePath = Path.Combine(
            folder,
            $"gpt_snapshots_{DateTime.UtcNow:yyyyMMdd}.jsonl");
    }

    public async Task AppendAsync(
        GptMarketSnapshot snapshot,
        CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(snapshot);

        await File.AppendAllTextAsync(
            _filePath,
            json + Environment.NewLine,
            ct);
    }
}