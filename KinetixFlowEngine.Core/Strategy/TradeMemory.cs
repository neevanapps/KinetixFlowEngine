using KinetixFlowEngine.Core.Strategy;

public class TradeMemory
{
    public string StrategyName { get; set; } = "";

    public SignalDirection Direction { get; set; }

    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }

    public string ExitReason { get; set; } = "";

    public DateTime ExitTime { get; set; }

    public Guid? CurrentPayloadId { get; set; }

    public Guid? QuantIntentId { get; set; }
}
