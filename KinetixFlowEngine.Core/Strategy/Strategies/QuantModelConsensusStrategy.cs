using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Quant;
using KinetixFlowEngine.Core.Trading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Strategy.Strategies;

internal sealed class QuantModelConsensusStrategy : IKinetixStrategy
{
    private readonly IQuantModelConsensusProvider _consensusProvider;
    private readonly StrategyConfig _config;
    private readonly QuantModelConsensusStrategyOptions _options;
    private readonly ILogger<QuantModelConsensusStrategy> _logger;

    public string Name => "QuantModelConsensus";

    public QuantModelConsensusStrategy(
        IQuantModelConsensusProvider consensusProvider,
        StrategyConfigLoader configLoader,
        IOptions<QuantModelConsensusStrategyOptions> options,
        ILogger<QuantModelConsensusStrategy> logger)
    {
        _consensusProvider = consensusProvider;
        _config = configLoader.Get(Name);
        _options = options.Value;
        _logger = logger;
    }

    public StrategySignal EvaluateEntry(KinetixEngineResult result)
    {
        if (!_options.Enabled || !_config.Enabled)
            return NoSignal();

        var consensus = _consensusProvider.GetLatest();

        if (consensus is null)
            return NoSignal("No Quant consensus cached.");

        if (!consensus.IsAvailable)
            return NoSignal($"Quant consensus unavailable: {consensus.BlockReason}");

        if (IsStale(consensus))
            return NoSignal("Quant consensus is stale.");

        if (_options.RequireShouldTradeForEntry && !consensus.ShouldTrade)
            return NoSignal($"Quant temporal consensus says no trade. Block={consensus.BlockReason}");

        if (consensus.RecommendedAction == "HOLD")
            return NoSignal($"Quant temporal consensus action is HOLD. Block={consensus.BlockReason}");

        var direction = ResolveSignalDirection(consensus);

        if (direction == SignalDirection.None)
        {
            return NoSignal(
                $"Unsupported Quant temporal direction/action. Direction={consensus.Direction}, Action={consensus.RecommendedAction}");
        }

        var confidence = (double)Math.Abs(consensus.WeightedDirectionalScore);

        if (_config.MinConfidence > 0 && confidence < _config.MinConfidence)
        {
            return NoSignal(
                $"Temporal consensus confidence below strategy config. Confidence={confidence:F2}, Required={_config.MinConfidence:F2}");
        }

        _logger.LogInformation(
            "QuantModelConsensus ENTRY signal | Direction={Direction} | TemporalScore={TemporalScore} | CurrentScore={CurrentScore} | ThreeAgree={ThreeAgree} | CurrentAgreement={CurrentAgreement} | ExecutableVotes={ExecutableVotes} | ExecutableBatches={ExecutableBatches} | CurrentPayload={CurrentPayload} | PreviousPayload={PreviousPayload} | ThirdPayload={ThirdPayload}",
            direction,
            consensus.WeightedDirectionalScore,
            consensus.CurrentBatchWeightedDirectionalScore,
            consensus.ThreeDirectionsAgree,
            consensus.CurrentBatchDirectionalAgreementRatio,
            consensus.CurrentBatchExecutableVoteCount,
            consensus.ExecutableBatchCount,
            consensus.CurrentPayloadId,
            consensus.PreviousPayloadId,
            consensus.ThirdPayloadId);

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
        if (!_options.Enabled || !_config.Enabled)
            return NoSignal();

        if (!_options.EnableExitOnOppositeConsensus)
            return NoSignal();

        var consensus = _consensusProvider.GetLatest();

        if (consensus is null || !consensus.IsAvailable)
            return NoSignal();

        if (IsStale(consensus))
            return NoSignal();

        // Exit behavior intentionally remains based on the latest completed
        // batch, not the new three-batch temporal entry confirmation.
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

        if (trade.Direction == SignalDirection.Long &&
            consensus.CurrentBatchDirection.Equals(
                "SHORT",
                StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "QuantModelConsensus EXIT LONG signal | CurrentScore={Score} | Agreement={Agreement} | Payload={PayloadId}",
                consensus.CurrentBatchWeightedDirectionalScore,
                consensus.CurrentBatchDirectionalAgreementRatio,
                consensus.CurrentPayloadId);

            return new StrategySignal
            {
                StrategyName = Name,
                ExitSignal = true
            };
        }

        if (trade.Direction == SignalDirection.Short &&
            consensus.CurrentBatchDirection.Equals(
                "LONG",
                StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "QuantModelConsensus EXIT SHORT signal | CurrentScore={Score} | Agreement={Agreement} | Payload={PayloadId}",
                consensus.CurrentBatchWeightedDirectionalScore,
                consensus.CurrentBatchDirectionalAgreementRatio,
                consensus.CurrentPayloadId);

            return new StrategySignal
            {
                StrategyName = Name,
                ExitSignal = true
            };
        }

        return NoSignal();
    }

    private bool IsStale(QuantModelConsensusDecision consensus)
    {
        var maxAge = TimeSpan.FromSeconds(
            Math.Max(60, _options.MaxConsensusAgeSeconds));

        return DateTimeOffset.UtcNow - consensus.LatestCreatedUtc > maxAge;
    }

    private static SignalDirection ResolveSignalDirection(
        QuantModelConsensusDecision consensus)
    {
        if (consensus.RecommendedAction.Equals(
                "ENTER_LONG",
                StringComparison.OrdinalIgnoreCase) &&
            consensus.Direction.Equals(
                "LONG",
                StringComparison.OrdinalIgnoreCase))
        {
            return SignalDirection.Long;
        }

        if (consensus.RecommendedAction.Equals(
                "ENTER_SHORT",
                StringComparison.OrdinalIgnoreCase) &&
            consensus.Direction.Equals(
                "SHORT",
                StringComparison.OrdinalIgnoreCase))
        {
            return SignalDirection.Short;
        }

        return SignalDirection.None;
    }

    private StrategySignal NoSignal(string? reason = null)
    {
        if (_options.LogNoSignalReason && !string.IsNullOrWhiteSpace(reason))
            _logger.LogDebug("QuantModelConsensus no signal: {Reason}", reason);

        return new StrategySignal
        {
            StrategyName = Name,
            Direction = SignalDirection.None
        };
    }
}
