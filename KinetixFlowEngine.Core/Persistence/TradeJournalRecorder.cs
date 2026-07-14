using System.Globalization;

namespace KinetixFlowEngine.Core.Persistence
{
    public class TradeJournalRecorder
    {
        private const string LegacyHeader =
            "timestamp,strategy,direction,entry,exit,stop,target1,target1hit,size,duration_sec,pnl_usd,gross_pnl_usd,fee_usd,pnl_r,mfe,mae,score_z,velocity_z,imbalance_z,compression_z,atr,er,flow_state";

        private const string ExtendedColumns =
            "trade_id,account_id,order_id,quant_intent_id,current_payload_id,previous_payload_id,third_payload_id,consensus_decision_utc,signal_utc,pending_intent_created_utc,entry_utc,review_count,current_batch_score,temporal_score,executable_votes,directional_agreement,executable_agreement,executable_batch_count,review_span_minutes,market_price_at_signal,fair_price_at_signal,fair_price_at_entry,entry_delay_sec,intent_expiry_reason,exit_reason";

        private static readonly string Header = $"{LegacyHeader},{ExtendedColumns}";

        private readonly string _filePath;
        private readonly object _sync = new();

        public TradeJournalRecorder()
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "journal");
            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "trade_journal.csv");
            EnsureHeader();
        }

        private void EnsureHeader()
        {
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, Header + Environment.NewLine);
                return;
            }

            var lines = File.ReadAllLines(_filePath);

            if (lines.Length == 0)
            {
                File.WriteAllText(_filePath, Header + Environment.NewLine);
                return;
            }

            if (lines[0].Equals(Header, StringComparison.Ordinal))
                return;

            if (lines[0].Equals(LegacyHeader, StringComparison.Ordinal))
            {
                lines[0] = Header;
                File.WriteAllLines(_filePath, lines);
                return;
            }

            var backup = _filePath + ".header_mismatch_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            File.Move(_filePath, backup);
            File.WriteAllText(_filePath, Header + Environment.NewLine);
        }

        public void Record(TradeJournalRecord r)
        {
            var values = new[]
            {
                r.Timestamp.ToString("O", CultureInfo.InvariantCulture),
                r.Strategy,
                r.Direction.ToString(),
                r.EntryPrice.ToString(CultureInfo.InvariantCulture),
                r.ExitPrice.ToString(CultureInfo.InvariantCulture),
                r.StopLoss.ToString(CultureInfo.InvariantCulture),
                r.Target1.ToString(CultureInfo.InvariantCulture),
                r.Target1Hit ? "1" : "0",
                r.Size.ToString(CultureInfo.InvariantCulture),
                r.DurationSeconds.ToString(CultureInfo.InvariantCulture),
                r.PnlUsd.ToString(CultureInfo.InvariantCulture),
                r.GrossPnlUsd.ToString(CultureInfo.InvariantCulture),
                r.FeeUsd.ToString(CultureInfo.InvariantCulture),
                r.PnlR.ToString(CultureInfo.InvariantCulture),
                r.MFE.ToString(CultureInfo.InvariantCulture),
                r.MAE.ToString(CultureInfo.InvariantCulture),
                r.ScoreZ.ToString(CultureInfo.InvariantCulture),
                r.VelocityZ.ToString(CultureInfo.InvariantCulture),
                r.ImbalanceZ.ToString(CultureInfo.InvariantCulture),
                r.CompressionZ.ToString(CultureInfo.InvariantCulture),
                r.ATR.ToString(CultureInfo.InvariantCulture),
                r.ER.ToString(CultureInfo.InvariantCulture),
                r.FlowState,

                r.TradeId,
                r.AccountId,
                r.OrderId,
                r.QuantIntentId?.ToString() ?? string.Empty,
                r.CurrentPayloadId?.ToString() ?? string.Empty,
                r.PreviousPayloadId?.ToString() ?? string.Empty,
                r.ThirdPayloadId?.ToString() ?? string.Empty,
                r.ConsensusDecisionUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                r.SignalUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                r.PendingIntentCreatedUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                r.EntryUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                r.ReviewCount.ToString(CultureInfo.InvariantCulture),
                r.CurrentBatchScore.ToString(CultureInfo.InvariantCulture),
                r.TemporalScore.ToString(CultureInfo.InvariantCulture),
                r.ExecutableVotes.ToString(CultureInfo.InvariantCulture),
                r.DirectionalAgreement.ToString(CultureInfo.InvariantCulture),
                r.ExecutableAgreement.ToString(CultureInfo.InvariantCulture),
                r.ExecutableBatchCount.ToString(CultureInfo.InvariantCulture),
                r.ReviewSpanMinutes.ToString(CultureInfo.InvariantCulture),
                r.MarketPriceAtSignal.ToString(CultureInfo.InvariantCulture),
                r.FairPriceAtSignal.ToString(CultureInfo.InvariantCulture),
                r.FairPriceAtEntry.ToString(CultureInfo.InvariantCulture),
                r.EntryDelaySeconds.ToString(CultureInfo.InvariantCulture),
                r.IntentExpiryReason,
                r.ExitReason
            };

            var line = string.Join(",", values.Select(EscapeCsv));

            lock (_sync)
            {
                File.AppendAllText(_filePath, line + Environment.NewLine);
            }
        }

        private static string EscapeCsv(string? value)
        {
            var text = value ?? string.Empty;

            if (!text.Contains(',') && !text.Contains('"') &&
                !text.Contains('\r') && !text.Contains('\n'))
            {
                return text;
            }

            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }
    }
}
