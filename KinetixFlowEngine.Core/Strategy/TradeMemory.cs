using KinetixFlowEngine.Core.Strategy;

public class TradeMemory
{
    public string StrategyName { get; set; } = "";

    public SignalDirection Direction { get; set; }

    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }

    public string ExitReason { get; set; } = ""; // SL / TSL / TP / SignalFlip

    public DateTime ExitTime { get; set; }
}