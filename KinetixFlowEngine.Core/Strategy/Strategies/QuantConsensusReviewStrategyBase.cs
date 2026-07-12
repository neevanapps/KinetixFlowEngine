using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Quant;
using KinetixFlowEngine.Core.Trading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Strategy.Strategies;

internal abstract class QuantConsensusReviewStrategyBase : IKinetixStrategy
{
    private readonly IQuantModelConsensusProvider _consensusProvider;
    private readonly StrategyConfig _config;
    private readonly QuantModelConsensusStrategyOptions _options;
    private readonly ILogger _logger;

    protected QuantConsensusReviewStrategyBase(
        string name,
        IQuantModelConsensusProvider consensusProvider,
        StrategyConfigLoader configLoader,
        IOptions<QuantModelConsensusStrategyOptions> options,
        ILogger logger)
    {
        Name = name;
        _consensusProvider = consensusProvider;
        _config = configLoader.Get(name);
        _options = options.Value;
        _logger = logger;
    }

    public string Name { get; }

    protected QuantModelConsensusStrategyOptions Options => _options;

    protected abstract bool IsProfileEnabled { get; }

    protected abstract QuantConsensusEntryEvaluation EvaluateConsensus(
        QuantModelConsensusDecision consensus);

    public StrategySignal EvaluateEntry(KinetixEngineResult result)
    {
        if (!_options.Enabled || !IsProfileEnabled || !_config.Enabled)
            return NoSignal();

        var consensus = _consensusProvider.GetLatest();

        if (consensus is null)
            return NoSignal("No Quant consensus cached.");

        if (!consensus.IsAvailable)
            return NoSignal($"Quant consensus unavailable: {consensus.BlockReason}");

        if (IsStale(consensus))
            return NoSignal("Quant consensus is stale.");

        var evaluation = EvaluateConsensus(consensus);

        if (!evaluation.Approved)
            return NoSignal(evaluation.BlockReason);

        var direction = ResolveSignalDirection(evaluation.Direction);

        if (direction == SignalDirection.None)
        {
            return NoSignal(
                $"Unsupported Quant direction. Direction={evaluation.Direction}");
        }

        var confidence = (double)Math.Abs(evaluation.Score);

        if (_config.MinConfidence > 0 && confidence < _config.MinConfidence)
        {
            return NoSignal(
                $"Consensus score below strategy config. Score={confidence:F2}, Required={_config.MinConfidence:F2}");
        }

        _logger.LogInformation(
            "{Strategy} ENTRY signal | Reviews={Reviews} | Direction={Direction} | Score={Score} | CurrentAgreement={CurrentAgreement} | CurrentExecutableVotes={ExecutableVotes} | ExecutableBatches={ExecutableBatches} | SpanMinutes={SpanMinutes} | CurrentPayload={CurrentPayload} | PreviousPayload={PreviousPayload} | ThirdPayload={ThirdPayload}",
            Name,
            evaluation.ReviewCount,
            direction,
            evaluation.Score,
            evaluation.CurrentDirectionalAgreementRatio,
            evaluation.CurrentExecutableVoteCount,
            evaluation.ExecutableBatchCount,
            evaluation.ReviewSpanMinutes,
            evaluation.CurrentPayloadId,
            evaluation.PreviousPayloadId,
            evaluation.ThirdPayloadId);

        return new StrategySignal
        {
            StrategyName = Name,
            Direction = direction,
            Confidence = confidence,
            EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
            NotifyThroughTelegram = _config.NotifyThroughTelegram,
            IsVolumeBased = _config.VolumeBased,
            TargetAccountIds = _config.AccountIds ?? new List<string>()
        };
    }

    public StrategySignal EvaluateExit(KinetixEngineResult result, ActiveTrade trade)
    {
        if (!_options.Enabled || !IsProfileEnabled || !_config.Enabled)
            return NoSignal();

        if (!_options.EnableExitOnOppositeConsensus)
            return NoSignal();

        var consensus = _consensusProvider.GetLatest();

        if (consensus is null || !consensus.IsAvailable || IsStale(consensus))
            return NoSignal();

        // Exit remains intentionally based on the latest completed batch.
        // Entry confirmation depth (1/2/3 reviews) does not delay protection.
        if (_options.RequireShouldTradeForExit && !consensus.CurrentBatchShouldTrade)
            return NoSignal();

        if (Math.Abs(consensus.CurrentBatchWeightedDirectionalScore) <
            _options.MinExitDirectionalScore)
        {
            return NoSignal();
        }

        if (consensus.CurrentBatchDirectionalAgreementRatio <
            _options.MinExitAgreementRatio)
        {
            return NoSignal();
        }

        var isOpposite =
            trade.Direction == SignalDirection.Long &&
            consensus.CurrentBatchDirection.Equals(
                "SHORT",
                StringComparison.OrdinalIgnoreCase)
            ||
            trade.Direction == SignalDirection.Short &&
            consensus.CurrentBatchDirection.Equals(
                "LONG",
                StringComparison.OrdinalIgnoreCase);

        if (!isOpposite)
            return NoSignal();

        _logger.LogInformation(
            "{Strategy} EXIT signal | TradeDirection={TradeDirection} | CurrentDirection={CurrentDirection} | CurrentScore={Score} | Agreement={Agreement} | Payload={PayloadId}",
            Name,
            trade.Direction,
            consensus.CurrentBatchDirection,
            consensus.CurrentBatchWeightedDirectionalScore,
            consensus.CurrentBatchDirectionalAgreementRatio,
            consensus.CurrentPayloadId);

        return new StrategySignal
        {
            StrategyName = Name,
            ExitSignal = true
        };
    }

    protected static IReadOnlyList<QuantModelBatchConsensusDecision> GetLatestBatches(
        QuantModelConsensusDecision consensus,
        int count)
    {
        return consensus.BatchConsensuses
            .OrderByDescending(x => x.DecisionTimeUtc)
            .Take(count)
            .ToList();
    }

    protected static bool IsDirectional(string direction)
    {
        return direction.Equals("LONG", StringComparison.OrdinalIgnoreCase) ||
               direction.Equals("SHORT", StringComparison.OrdinalIgnoreCase);
    }

    protected static decimal CalculateWeightedScore(
        IReadOnlyList<(decimal Score, decimal Weight)> values)
    {
        var totalWeight = values.Sum(x => Math.Max(0m, x.Weight));

        if (totalWeight <= 0)
            return 0m;

        return values.Sum(x => x.Score * Math.Max(0m, x.Weight)) / totalWeight;
    }

    protected static decimal CalculateSpanMinutes(
        QuantModelBatchConsensusDecision newest,
        QuantModelBatchConsensusDecision oldest)
    {
        return Math.Max(
            0m,
            (decimal)(newest.DecisionTimeUtc - oldest.DecisionTimeUtc).TotalMinutes);
    }

    private bool IsStale(QuantModelConsensusDecision consensus)
    {
        var maxAge = TimeSpan.FromSeconds(
            Math.Max(60, _options.MaxConsensusAgeSeconds));

        return DateTimeOffset.UtcNow - consensus.LatestCreatedUtc > maxAge;
    }

    private static SignalDirection ResolveSignalDirection(string direction)
    {
        if (direction.Equals("LONG", StringComparison.OrdinalIgnoreCase))
            return SignalDirection.Long;

        if (direction.Equals("SHORT", StringComparison.OrdinalIgnoreCase))
            return SignalDirection.Short;

        return SignalDirection.None;
    }

    private StrategySignal NoSignal(string? reason = null)
    {
        if (_options.LogNoSignalReason && !string.IsNullOrWhiteSpace(reason))
            _logger.LogDebug("{Strategy} no signal: {Reason}", Name, reason);

        return new StrategySignal
        {
            StrategyName = Name,
            Direction = SignalDirection.None
        };
    }
}
