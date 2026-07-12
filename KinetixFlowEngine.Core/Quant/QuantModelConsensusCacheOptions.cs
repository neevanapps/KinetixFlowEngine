namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantModelConsensusCacheOptions
{
    public bool Enabled { get; set; } = true;

    public int RefreshIntervalSeconds { get; set; } = 15;

    public bool LogRefreshResult { get; set; } = true;
}