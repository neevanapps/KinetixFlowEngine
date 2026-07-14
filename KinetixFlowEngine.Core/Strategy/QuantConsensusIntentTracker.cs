using System.Text.Json;
using KinetixFlowEngine.Core.Persistence;

namespace KinetixFlowEngine.Core.Strategy;

public sealed class QuantConsensusIntentTracker
{
    private readonly Dictionary<string, QuantConsensusIntentState> _states;
    private readonly QuantConsensusIntentJournalRecorder _journal;
    private readonly ILogger<QuantConsensusIntentTracker> _logger;
    private readonly string _filePath;
    private readonly object _sync = new();

    public QuantConsensusIntentTracker(
        QuantConsensusIntentJournalRecorder journal,
        ILogger<QuantConsensusIntentTracker> logger)
    {
        _journal = journal;
        _logger = logger;

        var folder = Path.Combine(AppContext.BaseDirectory, "persist");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "quant_consensus_intents.json");

        _states = Load();
    }

    public QuantConsensusIntentState GetOrCreate(
        string strategyName,
        SignalDirection direction,
        Guid currentPayloadId,
        Guid? previousPayloadId,
        Guid? thirdPayloadId,
        DateTimeOffset consensusDecisionUtc,
        int reviewCount,
        decimal currentBatchScore,
        decimal temporalScore,
        int executableVotes,
        decimal directionalAgreement,
        decimal executableAgreement,
        int executableBatchCount,
        decimal reviewSpanMinutes,
        decimal marketPriceAtSignal,
        decimal fairPriceAtSignal)
    {
        lock (_sync)
        {
            if (_states.TryGetValue(strategyName, out var existing))
            {
                if (existing.CurrentPayloadId == currentPayloadId &&
                    existing.Direction == direction)
                {
                    return existing.Clone();
                }

                if (existing.Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase))
                {
                    ExpireInternal(
                        existing,
                        existing.Direction == direction
                            ? "NEW_PAYLOAD"
                            : "DIRECTION_CHANGE");
                }
            }

            var now = DateTimeOffset.UtcNow;

            var created = new QuantConsensusIntentState
            {
                IntentId = Guid.NewGuid(),
                StrategyName = strategyName,
                Direction = direction,
                Status = "PENDING",
                CurrentPayloadId = currentPayloadId,
                PreviousPayloadId = previousPayloadId,
                ThirdPayloadId = thirdPayloadId,
                ConsensusDecisionUtc = consensusDecisionUtc,
                SignalUtc = now,
                PendingIntentCreatedUtc = now,
                ReviewCount = reviewCount,
                CurrentBatchScore = currentBatchScore,
                TemporalScore = temporalScore,
                ExecutableVotes = executableVotes,
                DirectionalAgreement = directionalAgreement,
                ExecutableAgreement = executableAgreement,
                ExecutableBatchCount = executableBatchCount,
                ReviewSpanMinutes = reviewSpanMinutes,
                MarketPriceAtSignal = marketPriceAtSignal,
                FairPriceAtSignal = fairPriceAtSignal
            };

            _states[strategyName] = created;
            Save();
            _journal.Record("CREATED", created);

            _logger.LogInformation(
                "Quant consensus intent created | Strategy={Strategy} | Intent={IntentId} | Direction={Direction} | Payload={PayloadId} | FairPrice={FairPrice}",
                strategyName,
                created.IntentId,
                direction,
                currentPayloadId,
                fairPriceAtSignal);

            return created.Clone();
        }
    }

    public void InvalidatePending(
        string strategyName,
        Guid? currentPayloadId,
        string reason)
    {
        lock (_sync)
        {
            if (!_states.TryGetValue(strategyName, out var state))
                return;

            if (!state.Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase))
                return;

            if (currentPayloadId.HasValue && state.CurrentPayloadId == currentPayloadId.Value)
            {
                ExpireInternal(state, reason);
                return;
            }

            if (!currentPayloadId.HasValue || state.CurrentPayloadId != currentPayloadId.Value)
                ExpireInternal(state, reason);
        }
    }

    public void MarkExecuted(
        StrategySignal signal,
        string accountId,
        string orderId,
        DateTimeOffset entryUtc,
        decimal entryPrice,
        decimal fairPriceAtEntry)
    {
        if (!signal.QuantIntentId.HasValue)
            return;

        lock (_sync)
        {
            if (!_states.TryGetValue(signal.StrategyName, out var state))
                return;

            if (state.IntentId != signal.QuantIntentId.Value)
                return;

            var executed = state.Clone();
            executed.EntryUtc = entryUtc;
            executed.EntryPrice = entryPrice;
            executed.FairPriceAtEntry = fairPriceAtEntry;
            executed.EntryDelaySeconds = Math.Max(
                0,
                (entryUtc - executed.SignalUtc).TotalSeconds);
            executed.AccountId = accountId;
            executed.OrderId = orderId;

            // The intent lifecycle is driven by the always-on SIM execution.
            // Real/demo account executions retain the same lineage on ActiveTrade,
            // but do not close the paper intent independently.
            if (accountId.Equals("SIM", StringComparison.OrdinalIgnoreCase))
            {
                executed.Status = "EXECUTED";
                _states[signal.StrategyName] = executed;
                Save();
            }

            _journal.Record("EXECUTED", executed);

            _logger.LogInformation(
                "Quant consensus intent executed | Strategy={Strategy} | Intent={IntentId} | Account={AccountId} | Entry={EntryPrice} | DelaySeconds={DelaySeconds}",
                signal.StrategyName,
                executed.IntentId,
                accountId,
                entryPrice,
                executed.EntryDelaySeconds);
        }
    }

    public bool IsExecuted(string strategyName, Guid intentId)
    {
        lock (_sync)
        {
            return _states.TryGetValue(strategyName, out var state) &&
                   state.IntentId == intentId &&
                   state.Status.Equals("EXECUTED", StringComparison.OrdinalIgnoreCase);
        }
    }

    private void ExpireInternal(
        QuantConsensusIntentState state,
        string reason)
    {
        state.Status = "EXPIRED";
        state.IntentExpiryReason = reason;
        Save();
        _journal.Record("EXPIRED", state);

        _logger.LogInformation(
            "Quant consensus intent expired | Strategy={Strategy} | Intent={IntentId} | Payload={PayloadId} | Reason={Reason}",
            state.StrategyName,
            state.IntentId,
            state.CurrentPayloadId,
            reason);
    }

    private Dictionary<string, QuantConsensusIntentState> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new Dictionary<string, QuantConsensusIntentState>(StringComparer.OrdinalIgnoreCase);

            var json = File.ReadAllText(_filePath);
            var values = JsonSerializer.Deserialize<List<QuantConsensusIntentState>>(json)
                         ?? [];

            return values
                .Where(x => !string.IsNullOrWhiteSpace(x.StrategyName))
                .GroupBy(x => x.StrategyName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderByDescending(y => y.PendingIntentCreatedUtc).First(),
                    StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load Quant consensus intent state. Starting empty.");
            return new Dictionary<string, QuantConsensusIntentState>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(
            _states.Values.OrderBy(x => x.StrategyName),
            new JsonSerializerOptions { WriteIndented = true });

        var tempFile = _filePath + ".tmp";
        File.WriteAllText(tempFile, json);

        if (File.Exists(_filePath))
            File.Delete(_filePath);

        File.Move(tempFile, _filePath);
    }
}
