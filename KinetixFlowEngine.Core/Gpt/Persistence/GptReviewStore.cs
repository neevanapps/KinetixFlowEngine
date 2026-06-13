using System.Text;
using System.Text.Json;
using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Persistence;

public interface IGptReviewStore
{
    Task AppendAsync(
        GptReviewRecord review,
        CancellationToken ct = default);

    Task<IReadOnlyList<GptReviewRecord>>
        GetRecentReviewsAsync(
            int count,
            CancellationToken ct = default);
}

public sealed class GptReviewStore : IGptReviewStore
{
    private readonly string _folder;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = false
        };

    public GptReviewStore()
    {
        _folder = Path.Combine(
            AppContext.BaseDirectory,
            "persist",
            "gpt");

        Directory.CreateDirectory(_folder);
    }

    public async Task AppendAsync(
        GptReviewRecord review,
        CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(
            review,
            JsonOptions);

        await File.AppendAllTextAsync(
            GetFilePath(),
            json + Environment.NewLine,
            Encoding.UTF8,
            ct);
    }

    public async Task<IReadOnlyList<GptReviewRecord>>
        GetRecentReviewsAsync(
            int count,
            CancellationToken ct = default)
    {
        var filePath = GetFilePath();

        if (!File.Exists(filePath))
            return [];

        var lines = await File.ReadAllLinesAsync(
            filePath,
            ct);

        var recentLines = lines
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .TakeLast(count);

        var reviews = new List<GptReviewRecord>();

        foreach (var line in recentLines)
        {
            try
            {
                var review =
                    JsonSerializer.Deserialize<GptReviewRecord>(
                        line,
                        JsonOptions);

                if (review != null)
                    reviews.Add(review);
            }
            catch
            {
                // Ignore corrupted line
            }
        }

        return reviews;
    }

    private string GetFilePath()
    {
        return Path.Combine(
            _folder,
            $"gpt_reviews_{DateTime.UtcNow:yyyyMMdd}.jsonl");
    }
}