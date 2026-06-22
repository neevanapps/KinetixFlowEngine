using System.Text.Json;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class GptMarketStateManager
{
    private readonly string _filePath;

    private readonly List<GptMarketStateRow> _rows = [];

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true
        };

    public GptMarketStateManager()
    {
        var folder =
            Path.Combine(
                AppContext.BaseDirectory,
                "persist",
                "gpt");

        Directory.CreateDirectory(folder);

        _filePath =
            Path.Combine(
                folder,
                "gpt_market_state.json");

        Load();
    }

    public IReadOnlyList<GptMarketStateRow> Rows => _rows;

    public void Add(GptMarketStateRow row)
    {
        _rows.Add(row);

        while (_rows.Count > KinetixConstants.MaxDepthCount)
        {
            _rows.RemoveAt(0);
        }
    }

    public async Task SaveAsync(
        CancellationToken ct = default)
    {
        var json =
            JsonSerializer.Serialize(
                _rows,
                JsonOptions);

        await File.WriteAllTextAsync(
            _filePath,
            json,
            ct);
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
            return;

        try
        {
            var json =
                File.ReadAllText(_filePath);

            var rows =
                JsonSerializer.Deserialize<
                    List<GptMarketStateRow>>(json);

            if (rows != null)
            {
                _rows.Clear();

                _rows.AddRange(rows.TakeLast(KinetixConstants.MaxDepthCount));
            }
        }
        catch
        {
            // Ignore corrupt file
        }
    }
}