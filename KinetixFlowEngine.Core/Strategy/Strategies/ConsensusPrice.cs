using KinetixFlowEngine.Core.Quant;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Strategy.Strategies;

internal sealed class ConsensusPriceStrategy
    : QuantConsensusReviewStrategyBase
{
    public const string StrategyName = "ConsensusPrice";

    public ConsensusPriceStrategy(
        IQuantModelConsensusProvider consensusProvider,
        StrategyConfigLoader configLoader,
        IOptions<QuantModelConsensusStrategyOptions> options,
        FairPriceEngine fairPriceEngine,
        QuantConsensusIntentTracker intentTracker,
        ILogger<ConsensusPriceStrategy> logger)
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

    protected override bool IsProfileEnabled => true;

    protected override QuantConsensusEntryEvaluation EvaluateConsensus(
        QuantModelConsensusDecision consensus)
    {
        var current = GetLatestBatches(consensus, 1).FirstOrDefault();

        if (current is null)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                1,
                "Latest completed Quant batch is unavailable.");
        }

        // Experiment rule is unchanged:
        // At least three LONG directional votes and score >= 20.
        if (current.LongDirectionalVoteCount >= 3 &&
            current.WeightedDirectionalScore >= 20m)
        {
            return Approved(
                current,
                "LONG");
        }

        // Experiment rule is unchanged:
        // At least three SHORT directional votes and score <= -20.
        if (current.ShortDirectionalVoteCount >= 3 &&
            current.WeightedDirectionalScore <= -20m)
        {
            return Approved(
                current,
                "SHORT");
        }

        return QuantConsensusEntryEvaluation.Blocked(
            1,
            $"Consensus experiment rule not satisfied. " +
            $"LongVotes={current.LongDirectionalVoteCount}, " +
            $"ShortVotes={current.ShortDirectionalVoteCount}, " +
            $"Score={current.WeightedDirectionalScore}.");
    }

    protected override bool ShouldExit(
        QuantModelConsensusDecision consensus,
        ActiveTrade trade)
    {
        var current = GetLatestBatches(consensus, 1).FirstOrDefault();

        if (current is null)
            return false;

        // Original exit rule for a LONG trade:
        // three SHORT directional votes and score <= -20.
        if (trade.Direction == SignalDirection.Long)
        {
            return current.ShortDirectionalVoteCount >= 3 &&
                   current.WeightedDirectionalScore <= -20m;
        }

        // Original exit rule for a SHORT trade:
        // three LONG directional votes and score >= 20.
        if (trade.Direction == SignalDirection.Short)
        {
            return current.LongDirectionalVoteCount >= 3 &&
                   current.WeightedDirectionalScore >= 20m;
        }

        return false;
    }

    private static QuantConsensusEntryEvaluation Approved(
        QuantModelBatchConsensusDecision current,
        string direction)
    {
        return new QuantConsensusEntryEvaluation
        {
            Approved = true,
            Direction = direction,
            Score = current.WeightedDirectionalScore,
            CurrentBatchScore = current.WeightedDirectionalScore,

            ReviewCount = 1,
            ReviewSpanMinutes = 0m,

            ExecutableBatchCount = current.ShouldTrade ? 1 : 0,
            CurrentExecutableVoteCount = current.ExecutableVoteCount,
            CurrentDirectionalAgreementRatio =
                current.DirectionalAgreementRatio,
            CurrentExecutableAgreementRatio =
                current.ExecutableAgreementRatio,

            CurrentPayloadId = current.PayloadId,
            ConsensusDecisionUtc = current.DecisionTimeUtc
        };
    }
}