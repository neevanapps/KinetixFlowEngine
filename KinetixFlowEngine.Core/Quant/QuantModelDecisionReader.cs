using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantModelDecisionReader : IQuantModelDecisionReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly QuantDecisionReaderOptions _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<QuantModelDecisionReader> _logger;

    public QuantModelDecisionReader(
        IOptions<QuantDecisionReaderOptions> options,
        IConfiguration configuration,
        ILogger<QuantModelDecisionReader> logger)
    {
        _options = options.Value;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<QuantModelDecisionBatch?> GetLatestCompleteBatchAsync(
        CancellationToken cancellationToken)
    {
        var batches = await GetLatestCompleteBatchesAsync(cancellationToken);
        return batches.FirstOrDefault();
    }

    public async Task<IReadOnlyList<QuantModelDecisionBatch>> GetLatestCompleteBatchesAsync(
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return [];

        var connectionString = ResolveConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning(
                "Quant decision reader skipped. Missing connection string. Name={Name}",
                _options.ConnectionStringName);

            return [];
        }

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var candidates = await GetLatestPayloadCandidatesAsync(
                connection,
                cancellationToken);

            if (candidates.Count == 0)
                return [];

            var result = new List<QuantModelDecisionBatch>();

            foreach (var candidate in candidates)
            {
                var decisions = await GetDecisionsByPayloadAsync(
                    connection,
                    candidate.PayloadId,
                    cancellationToken);

                var batch = BuildCompletedBatch(decisions, candidate.DecisionTimeUtc);

                if (batch is null)
                    continue;

                result.Add(batch);

                if (result.Count >= Math.Max(1, _options.LatestBatchCount))
                    break;
            }

            return result
                .OrderByDescending(x => x.DecisionTimeUtc)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to read Quant model decisions from QuantDB.");

            return [];
        }
    }

    private QuantModelDecisionBatch? BuildCompletedBatch(
        IReadOnlyList<QuantModelDecision> sourceDecisions,
        DateTimeOffset candidateDecisionTimeUtc)
    {
        if (sourceDecisions.Count == 0)
            return null;

        var distinctLatestByModel = sourceDecisions
            .Where(x => !string.IsNullOrWhiteSpace(x.ModelName))
            .GroupBy(x => x.ModelName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(x => x.CreatedUtc)
                .First())
            .ToList();

        var expectedNames = ResolveExpectedModelNames();
        var expectedModelCount = expectedNames.Count > 0
            ? expectedNames.Count
            : Math.Max(1, _options.ExpectedModelCount);

        var unexpectedModels = expectedNames.Count == 0
            ? new List<string>()
            : distinctLatestByModel
                .Where(x => !expectedNames.Contains(x.ModelName))
                .Select(x => x.ModelName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

        var expectedDecisions = expectedNames.Count == 0
            ? distinctLatestByModel
            : distinctLatestByModel
                .Where(x => expectedNames.Contains(x.ModelName))
                .ToList();

        var decisionTimeUtc = expectedDecisions.Count > 0
            ? expectedDecisions.Max(x => x.DecisionTimeUtc)
            : candidateDecisionTimeUtc;

        var timeout = TimeSpan.FromSeconds(
            Math.Max(1, _options.BatchCompletionTimeoutSeconds));

        var timeoutCutoffUtc = decisionTimeUtc + timeout;

        var terminalExpected = expectedDecisions
            .Where(IsTerminalDecision)
            .ToList();

        var allExpectedTerminal =
            expectedDecisions.Count >= expectedModelCount &&
            terminalExpected.Count >= expectedModelCount;

        var decisionsAvailableAtTimeout = expectedDecisions
            .Where(x => x.CreatedUtc <= timeoutCutoffUtc)
            .Where(IsTerminalDecision)
            .OrderBy(x => x.ModelName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var allExpectedTerminalByTimeout =
            decisionsAvailableAtTimeout.Count >= expectedModelCount;

        var successfulAtTimeout = decisionsAvailableAtTimeout.Count(x => x.IsSuccess);

        IReadOnlyList<QuantModelDecision> frozenDecisions;
        DateTimeOffset completionUtc;
        string completionMode;

        if (allExpectedTerminalByTimeout)
        {
            frozenDecisions = decisionsAvailableAtTimeout;
            completionUtc = frozenDecisions.Max(x => x.CreatedUtc);
            completionMode = "ALL_EXPECTED_TERMINAL";
        }
        else if (DateTimeOffset.UtcNow >= timeoutCutoffUtc &&
                 successfulAtTimeout >= Math.Max(1, _options.MinValidModelCount))
        {
            // Once a timeout-completed batch becomes eligible, later model rows
            // are deliberately ignored so the batch cannot change after use.
            frozenDecisions = decisionsAvailableAtTimeout;
            completionUtc = timeoutCutoffUtc;
            completionMode = "TIMEOUT_FROZEN";
        }
        else if (allExpectedTerminal)
        {
            // If the timeout snapshot did not contain enough successful rows,
            // allow the batch only after all expected models eventually finish.
            frozenDecisions = expectedDecisions
                .OrderBy(x => x.ModelName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            completionUtc = frozenDecisions.Max(x => x.CreatedUtc);
            completionMode = "ALL_EXPECTED_TERMINAL_LATE";
        }
        else
        {
            return null;
        }

        var successfulModelCount = frozenDecisions.Count(x => x.IsSuccess);

        if (successfulModelCount < Math.Max(1, _options.MinValidModelCount))
            return null;

        var frozenNames = frozenDecisions
            .Select(x => x.ModelName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingExpectedModels = expectedNames.Count == 0
            ? new List<string>()
            : expectedNames
                .Where(x => !frozenNames.Contains(x))
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

        return new QuantModelDecisionBatch
        {
            PayloadId = frozenDecisions.First().PayloadId,
            Symbol = frozenDecisions.First().Symbol,
            DecisionTimeUtc = decisionTimeUtc,
            LatestCreatedUtc = frozenDecisions.Max(x => x.CreatedUtc),
            CompletionUtc = completionUtc,
            CompletionMode = completionMode,
            ExpectedModelCount = expectedModelCount,
            ObservedExpectedModelCount = frozenDecisions.Count,
            TerminalModelCount = frozenDecisions.Count(IsTerminalDecision),
            SuccessfulModelCount = successfulModelCount,
            IsComplete = true,
            MissingExpectedModels = missingExpectedModels,
            UnexpectedModels = unexpectedModels,
            Decisions = frozenDecisions
        };
    }

    private HashSet<string> ResolveExpectedModelNames()
    {
        return _options.ExpectedModelNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private bool IsTerminalDecision(QuantModelDecision decision)
    {
        if (string.IsNullOrWhiteSpace(decision.Status))
            return false;

        return _options.TerminalStatuses.Any(status =>
            status.Equals(decision.Status, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<IReadOnlyList<PayloadCandidate>> GetLatestPayloadCandidatesAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        var cutoffUtc = DateTimeOffset.UtcNow.AddMinutes(
            -Math.Max(1, _options.CandidateLookbackMinutes));

        const string sql = """
            SELECT
                payload_id,
                MAX(decision_time_utc) AS decision_time_utc
            FROM llm_model_decisions
            WHERE symbol = @symbol
              AND decision_time_utc >= @cutoff_utc
            GROUP BY payload_id
            ORDER BY MAX(decision_time_utc) DESC
            LIMIT @limit;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("symbol", _options.Symbol);
        command.Parameters.AddWithValue("cutoff_utc", cutoffUtc);
        command.Parameters.AddWithValue(
            "limit",
            Math.Max(
                Math.Max(3, _options.LatestBatchCount),
                _options.CandidateBatchScanCount));

        var candidates = new List<PayloadCandidate>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            candidates.Add(new PayloadCandidate(
                reader.GetGuid(reader.GetOrdinal("payload_id")),
                GetDateTimeOffset(reader, "decision_time_utc")));
        }

        return candidates;
    }

    private static async Task<IReadOnlyList<QuantModelDecision>> GetDecisionsByPayloadAsync(
        NpgsqlConnection connection,
        Guid payloadId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                decision_id,
                payload_id,
                symbol,
                decision_time_utc,
                created_utc,
                model_name,
                provider,
                status,
                bias_direction,
                recommended_action,
                should_trade,
                long_confidence,
                short_confidence,
                directional_score,
                risk_level,
                tradeability,
                dominant_intent,
                time_horizon_minutes,
                decision_reason,
                supporting_evidence_json,
                opposing_evidence_json,
                invalidation_conditions_json,
                raw_response_json,
                parsed_response_json,
                error_message,
                latency_ms
            FROM llm_model_decisions
            WHERE payload_id = @payload_id
            ORDER BY created_utc ASC;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("payload_id", payloadId);

        var decisions = new List<QuantModelDecision>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            decisions.Add(new QuantModelDecision
            {
                DecisionId = GetGuid(reader, "decision_id"),
                PayloadId = GetGuid(reader, "payload_id"),
                Symbol = GetString(reader, "symbol", "BTCUSDT"),
                DecisionTimeUtc = GetDateTimeOffset(reader, "decision_time_utc"),
                CreatedUtc = GetDateTimeOffset(reader, "created_utc"),
                ModelName = GetString(reader, "model_name", string.Empty),
                Provider = GetString(reader, "provider", string.Empty),
                Status = GetString(reader, "status", string.Empty),
                BiasDirection = NormalizeDirection(
                    GetString(reader, "bias_direction", "Unknown")),
                RecommendedAction = NormalizeAction(
                    GetString(reader, "recommended_action", "HOLD")),
                ShouldTrade = GetBool(reader, "should_trade"),
                LongConfidence = GetInt(reader, "long_confidence"),
                ShortConfidence = GetInt(reader, "short_confidence"),
                DirectionalScore = GetInt(reader, "directional_score"),
                RiskLevel = NormalizeRisk(
                    GetString(reader, "risk_level", "Unknown")),
                Tradeability = NormalizeTradeability(
                    GetString(reader, "tradeability", "MEDIUM")),
                DominantIntent = GetString(reader, "dominant_intent", "UNCLEAR"),
                TimeHorizonMinutes = GetInt(reader, "time_horizon_minutes"),
                DecisionReason = GetString(reader, "decision_reason", string.Empty),
                SupportingEvidence = ReadStringArray(
                    GetString(reader, "supporting_evidence_json", "[]")),
                OpposingEvidence = ReadStringArray(
                    GetString(reader, "opposing_evidence_json", "[]")),
                InvalidationConditions = ReadStringArray(
                    GetString(reader, "invalidation_conditions_json", "[]")),
                RawResponseJson = GetString(reader, "raw_response_json", "{}"),
                ParsedResponseJson = GetString(reader, "parsed_response_json", "{}"),
                ErrorMessage = GetString(reader, "error_message", string.Empty),
                LatencyMs = GetInt(reader, "latency_ms")
            });
        }

        return decisions;
    }

    private string ResolveConnectionString()
    {
        var connectionString = _configuration.GetConnectionString(
            _options.ConnectionStringName);

        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        return _configuration[$"ConnectionStrings:{_options.ConnectionStringName}"]
            ?? string.Empty;
    }

    private static IReadOnlyList<string> ReadStringArray(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<string>>(
                json,
                JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static Guid GetGuid(NpgsqlDataReader reader, string column)
    {
        var value = reader.GetValue(reader.GetOrdinal(column));

        return value switch
        {
            Guid guid => guid,
            string text when Guid.TryParse(text, out var parsed) => parsed,
            _ => Guid.Empty
        };
    }

    private static string GetString(
        NpgsqlDataReader reader,
        string column,
        string fallback)
    {
        var ordinal = reader.GetOrdinal(column);

        if (reader.IsDBNull(ordinal))
            return fallback;

        return reader.GetString(ordinal);
    }

    private static int GetInt(NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);

        if (reader.IsDBNull(ordinal))
            return 0;

        var value = reader.GetValue(ordinal);

        return value switch
        {
            int i => i,
            long l => (int)l,
            short s => s,
            decimal d => (int)d,
            _ => int.TryParse(value.ToString(), out var parsed) ? parsed : 0
        };
    }

    private static bool GetBool(NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);

        if (reader.IsDBNull(ordinal))
            return false;

        var value = reader.GetValue(ordinal);

        return value switch
        {
            bool b => b,
            string text => bool.TryParse(text, out var parsed) && parsed,
            _ => false
        };
    }

    private static DateTimeOffset GetDateTimeOffset(
        NpgsqlDataReader reader,
        string column)
    {
        var ordinal = reader.GetOrdinal(column);

        if (reader.IsDBNull(ordinal))
            return DateTimeOffset.MinValue;

        var value = reader.GetValue(ordinal);

        return value switch
        {
            DateTimeOffset dto => dto.ToUniversalTime(),
            DateTime dt => new DateTimeOffset(
                DateTime.SpecifyKind(dt, DateTimeKind.Utc)),
            string text when DateTimeOffset.TryParse(text, out var parsed) =>
                parsed.ToUniversalTime(),
            _ => DateTimeOffset.MinValue
        };
    }

    private static string NormalizeDirection(string value)
    {
        if (value.Equals("LONG", StringComparison.OrdinalIgnoreCase))
            return "LONG";

        if (value.Equals("SHORT", StringComparison.OrdinalIgnoreCase))
            return "SHORT";

        return "UNKNOWN";
    }

    private static string NormalizeAction(string value)
    {
        if (value.Equals("ENTER_LONG", StringComparison.OrdinalIgnoreCase))
            return "ENTER_LONG";

        if (value.Equals("ENTER_SHORT", StringComparison.OrdinalIgnoreCase))
            return "ENTER_SHORT";

        return "HOLD";
    }

    private static string NormalizeRisk(string value)
    {
        if (value.Equals("LOW", StringComparison.OrdinalIgnoreCase))
            return "LOW";

        if (value.Equals("MEDIUM", StringComparison.OrdinalIgnoreCase))
            return "MEDIUM";

        if (value.Equals("HIGH", StringComparison.OrdinalIgnoreCase))
            return "HIGH";

        return "UNKNOWN";
    }

    private static string NormalizeTradeability(string value)
    {
        if (value.Equals("HIGH", StringComparison.OrdinalIgnoreCase))
            return "HIGH";

        if (value.Equals("LOW", StringComparison.OrdinalIgnoreCase))
            return "LOW";

        return "MEDIUM";
    }

    private sealed record PayloadCandidate(
        Guid PayloadId,
        DateTimeOffset DecisionTimeUtc);
}
