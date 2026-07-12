using KinetixFlowEngine.Core.Quant;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Strategy.Strategies;

internal sealed class QuantConsensus1ReviewStrategy : QuantConsensusReviewStrategyBase
{
    public const string StrategyName = "QuantConsensus1Review";

    public QuantConsensus1ReviewStrategy(
        IQuantModelConsensusProvider consensusProvider,
        StrategyConfigLoader configLoader,
        IOptions<QuantModelConsensusStrategyOptions> options,
        ILogger<QuantConsensus1ReviewStrategy> logger)
        : base(
            StrategyName,
            consensusProvider,
            configLoader,
            options,
            logger)
    {
    }

    protected override bool IsProfileEnabled => Options.OneReview.Enabled;

    protected override QuantConsensusEntryEvaluation EvaluateConsensus(
        QuantModelConsensusDecision consensus)
    {
        var batches = GetLatestBatches(consensus, 1);

        if (batches.Count < 1)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                1,
                "One completed Quant batch is required.");
        }

        var current = batches[0];
        var profile = Options.OneReview;

        if (!IsDirectional(current.Direction))
        {
            return QuantConsensusEntryEvaluation.Blocked(
                1,
                $"Current batch direction is not directional. Direction={current.Direction}.");
        }

        if (profile.RequireBatchShouldTrade && !current.ShouldTrade)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                1,
                $"Current batch is not executable. Reason={current.BlockReason}");
        }

        if (current.ExecutableVoteCount < profile.MinExecutableVotes)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                1,
                $"Current executable votes below threshold. Votes={current.ExecutableVoteCount}, Required={profile.MinExecutableVotes}.");
        }

        if (Math.Abs(current.WeightedDirectionalScore) < profile.MinDirectionalScore)
        {
            return QuantConsensusEntryEvaluation.Blocked(
                1,
                $"Current directional score below threshold. Score={current.WeightedDirectionalScore}, Required={profile.MinDirectionalScore}.");
        }

        return new QuantConsensusEntryEvaluation
        {
            Approved = true,
            Direction = current.Direction,
            Score = current.WeightedDirectionalScore,
            ReviewCount = 1,
            ReviewSpanMinutes = 0m,
            ExecutableBatchCount = current.ShouldTrade ? 1 : 0,
            CurrentExecutableVoteCount = current.ExecutableVoteCount,
            CurrentDirectionalAgreementRatio = current.DirectionalAgreementRatio,
            CurrentPayloadId = current.PayloadId
        };
    }
}
