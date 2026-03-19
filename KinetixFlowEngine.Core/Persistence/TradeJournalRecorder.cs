using System.Globalization;

namespace KinetixFlowEngine.Core.Persistence
{
    public class TradeJournalRecorder
    {
        private readonly string _filePath;

        public TradeJournalRecorder()
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "journal");
            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, $"trade_journal.csv");

            if (!File.Exists(_filePath))
                WriteHeader();
        }

        private void WriteHeader()
        {
            var header =
                "timestamp,strategy,direction,entry,exit,stop,target1,size,duration_sec,pnl_usd,gross_pnl_usd,fee_usd,pnl_r,mfe,mae,score_z,velocity_z,imbalance_z,compression_z,atr,er,flow_state";

            File.WriteAllText(_filePath, header + Environment.NewLine);
        }

        public void Record(TradeJournalRecord r)
        {
            var line = string.Join(",",
                r.Timestamp.ToString("O"),
                r.Strategy,
                r.Direction,

                r.EntryPrice.ToString(CultureInfo.InvariantCulture),
                r.ExitPrice.ToString(CultureInfo.InvariantCulture),
                r.StopLoss.ToString(CultureInfo.InvariantCulture),
                r.Target1.ToString(CultureInfo.InvariantCulture),

                r.Size.ToString(CultureInfo.InvariantCulture),                // ✅ NEW

                r.DurationSeconds.ToString(CultureInfo.InvariantCulture),

                r.PnlUsd.ToString(CultureInfo.InvariantCulture),              // ✅ renamed
                r.GrossPnlUsd.ToString(CultureInfo.InvariantCulture),         // ✅ NEW
                r.FeeUsd.ToString(CultureInfo.InvariantCulture),              // ✅ NEW

                r.PnlR.ToString(CultureInfo.InvariantCulture),                // ✅ fixed

                r.MFE.ToString(CultureInfo.InvariantCulture),
                r.MAE.ToString(CultureInfo.InvariantCulture),

                r.ScoreZ.ToString(CultureInfo.InvariantCulture),
                r.VelocityZ.ToString(CultureInfo.InvariantCulture),
                r.ImbalanceZ.ToString(CultureInfo.InvariantCulture),
                r.CompressionZ.ToString(CultureInfo.InvariantCulture),

                r.ATR.ToString(CultureInfo.InvariantCulture),
                r.ER.ToString(CultureInfo.InvariantCulture),

                r.FlowState
            );

            File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }
}