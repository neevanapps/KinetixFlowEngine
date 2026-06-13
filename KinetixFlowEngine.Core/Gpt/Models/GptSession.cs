namespace KinetixFlowEngine.Core.Gpt.Models;

public sealed class GptSession
{
    public DateOnly TradingDate { get; set; }

    public string ThreadId { get; set; } = string.Empty;

    public int NextSequence { get; set; }

    public bool Initialized { get; set; }

    public DateTime CreatedUtc { get; set; }
}

public static class EngineVersion
{
    public const string Version = "1.0.0";
}