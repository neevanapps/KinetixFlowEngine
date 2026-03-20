using KinetixFlowEngine.Core.Strategy;

public class TradeJournalRecord
{
    public string TradeId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Strategy { get; set; } = default!;
    public SignalDirection Direction { get; set; }

    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal Target1 { get; set; }
    public bool Target1Hit { get; set; }
    public decimal Size { get; set; }            // ✅ NEW

    public double DurationSeconds { get; set; }

    public decimal PnlUsd { get; set; }          // ✅ renamed from pnl_points
    public decimal GrossPnlUsd { get; set; }     // ✅ NEW
    public decimal FeeUsd { get; set; }          // ✅ NEW

    public decimal PnlR { get; set; }            // ✅ FIXED calc

    public decimal MFE { get; set; }
    public decimal MAE { get; set; }

    public double ScoreZ { get; set; }
    public double VelocityZ { get; set; }
    public double ImbalanceZ { get; set; }
    public double CompressionZ { get; set; }

    public decimal ATR { get; set; }
    public decimal ER { get; set; }

    public string FlowState { get; set; } = default!;
}