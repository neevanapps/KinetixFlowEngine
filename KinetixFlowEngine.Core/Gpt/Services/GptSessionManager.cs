using System.Text.Json;
using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class GptSessionManager : IGptSessionManager
{
    private readonly string _filePath;

    private GptSession _session;

    public GptSessionManager()
    {
        var folder = Path.Combine(
            AppContext.BaseDirectory,
            "persist",
            "gpt");

        Directory.CreateDirectory(folder);

        _filePath = Path.Combine(
            folder,
            "gpt_session.json");

        _session = LoadOrCreate();
    }

    public GptSession GetCurrentSession()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (_session.TradingDate != today)
        {
            _session = new GptSession
            {
                TradingDate = today,
                NextSequence = 1,
                Initialized = false,
                CreatedUtc = DateTime.UtcNow
            };

            Save();
        }

        return _session;
    }

    public int GetNextSequence()
    {
        var session = GetCurrentSession();

        var sequence = session.NextSequence;

        session.NextSequence++;

        Save();

        return sequence;
    }

    public bool RequiresInitialization()
    {
        return !GetCurrentSession().Initialized;
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(
            _session,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_filePath, json);
    }

    private GptSession LoadOrCreate()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return CreateNewSession();
            }

            var json = File.ReadAllText(_filePath);

            var session =
                JsonSerializer.Deserialize<GptSession>(json);

            return session ?? CreateNewSession();
        }
        catch
        {
            return CreateNewSession();
        }
    }

    private static GptSession CreateNewSession()
    {
        return new GptSession
        {
            TradingDate =
                DateOnly.FromDateTime(DateTime.UtcNow),

            NextSequence = 1,

            Initialized = false,

            CreatedUtc = DateTime.UtcNow
        };
    }

    public void MarkInitialized(string threadId)
    {
        _session.ThreadId = threadId;
        _session.Initialized = true;

        Save();
    }
}