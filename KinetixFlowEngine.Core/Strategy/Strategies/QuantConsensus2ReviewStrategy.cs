using KinetixFlowEngine.Core.Quant;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Strategy.Strategies;

internal sealed class QuantConsensus2ReviewStrategy : QuantConsensusReviewStrategyBase
{
    public const string StrategyName = "QuantConsensus2Review";

    public QuantConsensus2ReviewStrategy(
        IQuantModelConsensusProvider consensusProvider,
        StrategyConfigLoader configLoader,
        IOptions<QuantModelConsensusStrategyOptions> options,
        FairPriceEngine fairPriceEngine,
        QuantConsensusIntentTracker intentTracker,
        ILogger<QuantConsensus2ReviewStrategy> logger)
        : base(
            StrategyName,
            consensusProvider,
            configLoader,
            options,
            fairPriceEngine,
            intentTracker,
            logger)
    {
    }

    protected override bool IsProfileEnabled => Options.TwoReview.Enabled;

    protected override QuantConsensusEntryEvaluation EvaluateConsensus(
        QuantModelConsensusDecision consensus)
    {
        var batches = GetLatestBatches(consensus, 2);

        if (batches.Count < 2)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                2,
                $"Two completed Quant batches are required. Available={batches.Count}.");
        }

        var current = batches[0];
        var previous = batches[1];
        var profile = Options.TwoReview;

        if (!IsDirectional(current.Direction) || !IsDirectional(previous.Direction))
        {
            return QuantConsensusEntryEvaluation.Blocked(
                2,
                $"Both batches must be directional. Current={current.Direction}, Previous={previous.Direction}.");
        }

        if (!current.Direction.Equals(
                previous.Direction,
                StringComparison.OrdinalIgnoreCase))
        {
            return QuantConsensusEntryEvaluation.Blocked(
                2,
                $"Latest two batch directions do not agree. Current={current.Direction}, Previous={previous.Direction}.");
        }

        var spanMinutes = CalculateSpanMinutes(current, previous);

        if (spanMinutes > profile.MaxReviewSpanMinutes)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                2,
                $"Two-review span exceeds limit. SpanMinutes={Math.Round(spanMinutes, 2)}, Maximum={profile.MaxReviewSpanMinutes}.");
        }

        if (profile.RequireBothBatchesExecutable &&
            (!current.ShouldTrade || !previous.ShouldTrade))
        {
            return QuantConsensusEntryEvaluation.Blocked(
                2,
                $"Both latest batches must be executable. Current={current.ShouldTrade}, Previous={previous.ShouldTrade}.");
        }

        if (current.ExecutableVoteCount < profile.MinExecutableVotes)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                2,
                $"Current executable votes below threshold. Votes={current.ExecutableVoteCount}, Required={profile.MinExecutableVotes}.");
        }

        if (Math.Abs(current.WeightedDirectionalScore) <
            profile.MinCurrentDirectionalScore)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                2,
                $"Current directional score below threshold. Score={current.WeightedDirectionalScore}, Required={profile.MinCurrentDirectionalScore}.");
        }

        var weightedScore = CalculateWeightedScore(
        [
            (current.WeightedDirectionalScore, profile.CurrentBatchWeight),
            (previous.WeightedDirectionalScore, profile.PreviousBatchWeight)
        ]);

        if (Math.Abs(weightedScore) < profile.MinWeightedDirectionalScore)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                2,
                $"Two-review weighted score below threshold. Score={Math.Round(weightedScore, 2)}, Required={profile.MinWeightedDirectionalScore}.");
        }

        return new QuantConsensusEntryEvaluation
        {
            Approved = true,
            Direction = current.Direction,
            Score = Math.Round(weightedScore, 2),
            CurrentBatchScore = current.WeightedDirectionalScore,
            ReviewCount = 2,
            ReviewSpanMinutes = Math.Round(spanMinutes, 2),
            ExecutableBatchCount = batches.Count(x => x.ShouldTrade),
            CurrentExecutableVoteCount = current.ExecutableVoteCount,
            CurrentDirectionalAgreementRatio = current.DirectionalAgreementRatio,
            CurrentExecutableAgreementRatio = current.ExecutableAgreementRatio,
            CurrentPayloadId = current.PayloadId,
            PreviousPayloadId = previous.PayloadId,
            ConsensusDecisionUtc = current.DecisionTimeUtc
        };
    }
}
