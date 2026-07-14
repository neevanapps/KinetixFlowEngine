using System.Globalization;
using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Persistence;

public sealed class QuantConsensusIntentJournalRecorder
{
    private const string Header =
        "event_utc,event_type,intent_id,strategy,direction,status,current_payload_id,previous_payload_id,third_payload_id,consensus_decision_utc,signal_utc,pending_intent_created_utc,entry_utc,review_count,current_batch_score,temporal_score,executable_votes,directional_agreement,executable_agreement,executable_batch_count,review_span_minutes,market_price_at_signal,fair_price_at_signal,entry_price,fair_price_at_entry,entry_delay_sec,intent_expiry_reason,account_id,order_id";

    private readonly string _filePath;
    private readonly object _sync = new();

    public QuantConsensusIntentJournalRecorder()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "journal");
        Directory.CreateDirectory(folder);

        _filePath = Path.Combine(folder, "quant_consensus_intents.csv");

        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, Header + Environment.NewLine);
    }

    public void Record(string eventType, QuantConsensusIntentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var values = new[]
        {
            DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            eventType,
            state.IntentId.ToString(),
            state.StrategyName,
            state.Direction.ToString(),
            state.Status,
            state.CurrentPayloadId.ToString(),
            state.PreviousPayloadId?.ToString() ?? string.Empty,
            state.ThirdPayloadId?.ToString() ?? string.Empty,
            state.ConsensusDecisionUtc.ToString("O", CultureInfo.InvariantCulture),
            state.SignalUtc.ToString("O", CultureInfo.InvariantCulture),
            state.PendingIntentCreatedUtc.ToString("O", CultureInfo.InvariantCulture),
            state.EntryUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            state.ReviewCount.ToString(CultureInfo.InvariantCulture),
            state.CurrentBatchScore.ToString(CultureInfo.InvariantCulture),
            state.TemporalScore.ToString(CultureInfo.InvariantCulture),
            state.ExecutableVotes.ToString(CultureInfo.InvariantCulture),
            state.DirectionalAgreement.ToString(CultureInfo.InvariantCulture),
            state.ExecutableAgreement.ToString(CultureInfo.InvariantCulture),
            state.ExecutableBatchCount.ToString(CultureInfo.InvariantCulture),
            state.ReviewSpanMinutes.ToString(CultureInfo.InvariantCulture),
            state.MarketPriceAtSignal.ToString(CultureInfo.InvariantCulture),
            state.FairPriceAtSignal.ToString(CultureInfo.InvariantCulture),
            state.EntryPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            state.FairPriceAtEntry?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            state.EntryDelaySeconds?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            state.IntentExpiryReason,
            state.AccountId,
            state.OrderId
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
