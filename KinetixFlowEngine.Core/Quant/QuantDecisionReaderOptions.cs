namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantDecisionReaderOptions
{
    public bool Enabled { get; set; } = true;

    public string ConnectionStringName { get; set; } = "QuantDb";

    public string Symbol { get; set; } = "BTCUSDT";

    public int LatestBatchCount { get; set; } = 3;

    public int ExpectedModelCount { get; set; } = 4;

    public int MinValidModelCount { get; set; } = 3;

    public int BatchCompletionTimeoutSeconds { get; set; } = 90;

    public int CandidateLookbackMinutes { get; set; } = 720;

    public int CandidateBatchScanCount { get; set; } = 12;

    public List<string> ExpectedModelNames { get; set; } = [];

    public List<string> TerminalStatuses { get; set; } =
    [
        "Success",
        "Failed",
        "ParseFailed",
        "ValidationFailed"
    ];
}
