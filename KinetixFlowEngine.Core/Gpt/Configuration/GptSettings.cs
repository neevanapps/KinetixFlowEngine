namespace KinetixFlowEngine.Core.Gpt.Configuration;

public sealed class GptSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-5";
}