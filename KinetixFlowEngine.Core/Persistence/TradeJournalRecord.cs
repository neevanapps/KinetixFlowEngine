using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Persistence
{
    public class TradeJournalRecord
    {
        public DateTime Timestamp { get; set; }

        public string Strategy { get; set; } = "";

        public SignalDirection Direction { get; set; }

        public decimal EntryPrice { get; set; }

        public decimal ExitPrice { get; set; }

        public decimal StopLoss { get; set; }

        public decimal Target1 { get; set; }

        public long DurationSeconds { get; set; }

        public decimal PnlPoints { get; set; }

        public decimal PnlR { get; set; }

        public decimal MFE { get; set; }

        public decimal MAE { get; set; }

        public double ScoreZ { get; set; }

        public double VelocityZ { get; set; }

        public double ImbalanceZ { get; set; }

        public double CompressionZ { get; set; }

        public double ATR { get; set; }

        public double ER { get; set; }

        public string FlowState { get; set; } = "";
    }
}