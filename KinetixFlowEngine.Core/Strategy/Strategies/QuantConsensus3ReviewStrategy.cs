using KinetixFlowEngine.Core.Quant;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Strategy.Strategies;

internal sealed class QuantConsensus3ReviewStrategy : QuantConsensusReviewStrategyBase
{
    public const string StrategyName = "QuantConsensus3Review";

    public QuantConsensus3ReviewStrategy(
        IQuantModelConsensusProvider consensusProvider,
        StrategyConfigLoader configLoader,
        IOptions<QuantModelConsensusStrategyOptions> options,
        FairPriceEngine fairPriceEngine,
        QuantConsensusIntentTracker intentTracker,
        ILogger<QuantConsensus3ReviewStrategy> logger)
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

    protected override bool IsProfileEnabled => Options.ThreeReview.Enabled;

    protected override QuantConsensusEntryEvaluation EvaluateConsensus(
        QuantModelConsensusDecision consensus)
    {
        var batches = GetLatestBatches(consensus, 3);

        if (batches.Count < 3)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                3,
                $"Three completed Quant batches are required. Available={batches.Count}.");
        }

        var current = batches[0];
        var previous = batches[1];
        var third = batches[2];
        var profile = Options.ThreeReview;

        if (!IsDirectional(current.Direction) ||
            !IsDirectional(previous.Direction) ||
            !IsDirectional(third.Direction))
        {
            return QuantConsensusEntryEvaluation.Blocked(
                3,
                $"All three batches must be directional. Current={current.Direction}, Previous={previous.Direction}, Third={third.Direction}.");
        }

        var directionsAgree =
            current.Direction.Equals(
                previous.Direction,
                StringComparison.OrdinalIgnoreCase) &&
            current.Direction.Equals(
                third.Direction,
                StringComparison.OrdinalIgnoreCase);

        if (!directionsAgree)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                3,
                $"Three batch directions do not agree. Current={current.Direction}, Previous={previous.Direction}, Third={third.Direction}.");
        }

        var spanMinutes = CalculateSpanMinutes(current, third);

        if (spanMinutes > profile.MaxReviewSpanMinutes)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                3,
                $"Three-review span exceeds limit. SpanMinutes={Math.Round(spanMinutes, 2)}, Maximum={profile.MaxReviewSpanMinutes}.");
        }

        if (!current.ShouldTrade)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                3,
                $"Current batch is not executable. Reason={current.BlockReason}");
        }

        if (current.ExecutableVoteCount < profile.MinExecutableVotes)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                3,
                $"Current executable votes below threshold. Votes={current.ExecutableVoteCount}, Required={profile.MinExecutableVotes}.");
        }

        var executableBatchCount = batches.Count(x => x.ShouldTrade);

        if (executableBatchCount < profile.MinExecutableBatchCount)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                3,
                $"Executable batch count below threshold. Count={executableBatchCount}, Required={profile.MinExecutableBatchCount}.");
        }

        var weightedScore = CalculateWeightedScore(
        [
            (current.WeightedDirectionalScore, profile.CurrentBatchWeight),
            (previous.WeightedDirectionalScore, profile.PreviousBatchWeight),
            (third.WeightedDirectionalScore, profile.ThirdBatchWeight)
        ]);

        if (Math.Abs(weightedScore) < profile.MinWeightedDirectionalScore)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                3,
                $"Three-review weighted score below threshold. Score={Math.Round(weightedScore, 2)}, Required={profile.MinWeightedDirectionalScore}.");
        }

        return new QuantConsensusEntryEvaluation
        {
            Approved = true,
            Direction = current.Direction,
            Score = Math.Round(weightedScore, 2),
            CurrentBatchScore = current.WeightedDirectionalScore,
            ReviewCount = 3,
            ReviewSpanMinutes = Math.Round(spanMinutes, 2),
            ExecutableBatchCount = executableBatchCount,
            CurrentExecutableVoteCount = current.ExecutableVoteCount,
            CurrentDirectionalAgreementRatio = current.DirectionalAgreementRatio,
            CurrentExecutableAgreementRatio = current.ExecutableAgreementRatio,
            CurrentPayloadId = current.PayloadId,
            PreviousPayloadId = previous.PayloadId,
            ThirdPayloadId = third.PayloadId,
            ConsensusDecisionUtc = current.DecisionTimeUtc
        };
    }
}
