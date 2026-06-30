namespace KinetixFlowEngine.Core.Config;

public sealed class FlowEngineQuantExportOptions
{
    public bool Enabled { get; set; } = false;
    public string ConnectionStringName { get; set; } = "QuantDb";
    public int MaxQueueSize { get; set; } = 1000;
    public int RetryDelaySeconds { get; set; } = 10;
}
