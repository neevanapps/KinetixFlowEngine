using System.Text.Json;

namespace KinetixFlowEngine.Core.Depth;

public sealed class DepthFeatureManager
{
    private readonly string _filePath;

    public List<DepthMinuteFeature> Rows { get; } = new();

    public DepthFeatureManager()
    {
        Directory.CreateDirectory("data");

        _filePath =
            Path.Combine(
                "data",
                "depth-features.json");
    }

    public async Task LoadAsync(
        CancellationToken ct = default)
    {
        if (!File.Exists(_filePath))
            return;

        await using var stream =
            File.OpenRead(_filePath);

        var rows =
            await JsonSerializer.DeserializeAsync<
                List<DepthMinuteFeature>>(
                    stream,
                    cancellationToken: ct);

        if (rows == null)
            return;

        Rows.Clear();

        Rows.AddRange(rows);
    }

    public void Add(
        DepthMinuteFeature row)
    {
        Rows.Add(row);

        while (Rows.Count > 120)
        {
            Rows.RemoveAt(0);
        }
    }

    public async Task SaveAsync(
        CancellationToken ct = default)
    {
        await using var stream =
            File.Create(_filePath);

        await JsonSerializer.SerializeAsync(
            stream,
            Rows,
            new JsonSerializerOptions
            {
                WriteIndented = true
            },
            ct);
    }

    public IReadOnlyList<DepthMinuteFeature> GetRecent(
        int count)
    {
        return Rows
            .TakeLast(count)
            .ToList();
    }
}