using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantModelConsensusService : IQuantModelConsensusService
{
    private readonly IQuantModelDecisionReader _decisionReader;
    private readonly QuantModelConsensusOptions _options;
    private readonly ILogger<QuantModelConsensusService> _logger;

    public QuantModelConsensusService(
        IQuantModelDecisionReader decisionReader,
        IOptions<QuantModelConsensusOptions> options,
        ILogger<QuantModelConsensusService> logger)
    {
        _decisionReader = decisionReader;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<QuantModelConsensusDecision> GetLatestConsensusAsync(
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return QuantModelConsensusDecision.Unavailable("Quant model consensus disabled.");

        var batches = await _decisionReader.GetLatestCompleteBatchesAsync(cancellationToken);

        if (batches.Count == 0)
            return QuantModelConsensusDecision.Unavailable("No completed Quant model decision batches available.");

        var batchConsensuses = batches
            .OrderByDescending(x => x.DecisionTimeUtc)
            .Take(3)
            .Select(BuildBatchConsensus)
            .Where(x => x is not null)
            .Cast<QuantModelBatchConsensusDecision>()
            .ToList();

        if (batchConsensuses.Count == 0)
        {
            return QuantModelConsensusDecision.Unavailable(
                "Completed Quant batches did not contain enough schema-valid model decisions.");
        }

        return BuildTemporalConsensus(batchConsensuses);
    }

    private QuantModelBatchConsensusDecision? BuildBatchConsensus(
        QuantModelDecisionBatch batch)
    {
        var valid = batch.Decisions
            .Where(IsValidDecision)
            .ToList();

        if (valid.Count < Math.Max(1, _options.MinValidModelCount))
            return null;

        var weightedScore = CalculateWeightedDirectionalScore(valid);
        var direction = ResolveDirection(weightedScore);

        var longDirectionalVotes = valid.Count(x => x.IsLong);
        var shortDirectionalVotes = valid.Count(x => x.IsShort);

        var longExecutableVotes = valid.Count(x =>
            IsExecutableVoteForDirection(x, "LONG"));

        var shortExecutableVotes = valid.Count(x =>
            IsExecutableVoteForDirection(x, "SHORT"));

        var executableVoteCount = direction switch
        {
            "LONG" => longExecutableVotes,
            "SHORT" => shortExecutableVotes,
            _ => 0
        };

        var holdVotes = valid.Count(IsHoldDecision);

        var highRiskVotes = valid.Count(x =>
            x.RiskLevel.Equals("HIGH", StringComparison.OrdinalIgnoreCase));

        var lowTradeabilityVotes = valid.Count(x =>
            x.Tradeability.Equals("LOW", StringComparison.OrdinalIgnoreCase));

        var directionalAgreement = CalculateDirectionalAgreement(valid, direction);
        var executableAgreement = valid.Count == 0
            ? 0m
            : executableVoteCount / (decimal)valid.Count;

        var directionalSupportingModels = valid
            .Where(x => SupportsDirection(x, direction))
            .Select(x => x.ModelName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var executableSupportingModels = valid
            .Where(x => IsExecutableVoteForDirection(x, direction))
            .Select(x => x.ModelName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var opposingModels = valid
            .Where(x => OpposesDirection(x, direction))
            .Select(x => x.ModelName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var holdModels = valid
            .Where(IsHoldDecision)
            .Select(x => x.ModelName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var risk = ResolveConsensusRisk(valid);
        var tradeability = ResolveConsensusTradeability(valid);

        var blockReason = ResolveBatchBlockReason(
            direction,
            weightedScore,
            directionalAgreement,
            executableVoteCount,
            highRiskVotes,
            lowTradeabilityVotes);

        return new QuantModelBatchConsensusDecision
        {
            PayloadId = batch.PayloadId,
            DecisionTimeUtc = batch.DecisionTimeUtc,
            LatestCreatedUtc = batch.LatestCreatedUtc,
            CompletionMode = batch.CompletionMode,
            Direction = direction,
            WeightedDirectionalScore = Math.Round(weightedScore, 2),
            DirectionalAgreementRatio = Math.Round(directionalAgreement, 4),
            ExecutableAgreementRatio = Math.Round(executableAgreement, 4),
            LongDirectionalVoteCount = longDirectionalVotes,
            ShortDirectionalVoteCount = shortDirectionalVotes,
            LongExecutableVoteCount = longExecutableVotes,
            ShortExecutableVoteCount = shortExecutableVotes,
            ExecutableVoteCount = executableVoteCount,
            HoldVoteCount = holdVotes,
            HighRiskVoteCount = highRiskVotes,
            LowTradeabilityVoteCount = lowTradeabilityVotes,
            ValidModelCount = valid.Count,
            TotalModelCount = batch.Decisions.Count,
            RiskLevel = risk,
            Tradeability = tradeability,
            ShouldTrade = string.IsNullOrWhiteSpace(blockReason),
            BlockReason = blockReason,
            DirectionalSupportingModels = directionalSupportingModels,
            ExecutableSupportingModels = executableSupportingModels,
            OpposingModels = opposingModels,
            HoldModels = holdModels,
            ValidDecisions = valid
        };
    }

    private QuantModelConsensusDecision BuildTemporalConsensus(
        IReadOnlyList<QuantModelBatchConsensusDecision> batches)
    {
        var current = batches[0];
        var previous = batches.Count > 1 ? batches[1] : null;
        var third = batches.Count > 2 ? batches[2] : null;

        var hasThreeBatches = previous is not null && third is not null;

        var threeDirectionsAgree = hasThreeBatches &&
            IsDirectional(current.Direction) &&
            current.Direction.Equals(previous!.Direction, StringComparison.OrdinalIgnoreCase) &&
            current.Direction.Equals(third!.Direction, StringComparison.OrdinalIgnoreCase);

        var temporalScore = hasThreeBatches
            ? CalculateTemporalScore(current, previous!, third!)
            : 0m;

        var temporalSpanMinutes = hasThreeBatches
            ? Math.Max(
                0m,
                (decimal)(current.DecisionTimeUtc - third!.DecisionTimeUtc).TotalMinutes)
            : 0m;

        var executableBatchCount = batches.Count(x => x.ShouldTrade);

        var blockReason = ResolveTemporalBlockReason(
            current,
            previous,
            third,
            threeDirectionsAgree,
            temporalScore,
            temporalSpanMinutes,
            executableBatchCount);

        var temporalShouldTrade = string.IsNullOrWhiteSpace(blockReason);
        var temporalDirection = threeDirectionsAgree
            ? current.Direction
            : "NEUTRAL";

        var recommendedAction = temporalShouldTrade
            ? temporalDirection == "LONG" ? "ENTER_LONG" : "ENTER_SHORT"
            : "HOLD";

        var stability = ResolveTemporalStability(
            hasThreeBatches,
            threeDirectionsAgree,
            temporalSpanMinutes);

        if (!temporalShouldTrade)
        {
            _logger.LogDebug(
                "Temporal Quant consensus blocked. Current={CurrentPayload}, Previous={PreviousPayload}, Third={ThirdPayload}, Reason={Reason}",
                current.PayloadId,
                previous?.PayloadId,
                third?.PayloadId,
                blockReason);
        }

        return new QuantModelConsensusDecision
        {
            IsAvailable = true,
            CurrentPayloadId = current.PayloadId,
            PreviousPayloadId = previous?.PayloadId,
            ThirdPayloadId = third?.PayloadId,
            DecisionTimeUtc = current.DecisionTimeUtc,
            LatestCreatedUtc = current.LatestCreatedUtc,
            Direction = temporalDirection,
            RecommendedAction = recommendedAction,
            ShouldTrade = temporalShouldTrade,
            WeightedDirectionalScore = Math.Round(temporalScore, 2),
            AgreementRatio = current.DirectionalAgreementRatio,
            CurrentBatchDirection = current.Direction,
            PreviousBatchDirection = previous?.Direction ?? "UNAVAILABLE",
            ThirdBatchDirection = third?.Direction ?? "UNAVAILABLE",
            CurrentBatchWeightedDirectionalScore = current.WeightedDirectionalScore,
            CurrentBatchDirectionalAgreementRatio = current.DirectionalAgreementRatio,
            CurrentBatchExecutableAgreementRatio = current.ExecutableAgreementRatio,
            CurrentBatchExecutableVoteCount = current.ExecutableVoteCount,
            CurrentBatchShouldTrade = current.ShouldTrade,
            ThreeDirectionsAgree = threeDirectionsAgree,
            ExecutableBatchCount = executableBatchCount,
            TemporalSpanMinutes = Math.Round(temporalSpanMinutes, 2),
            LongVoteCount = current.LongDirectionalVoteCount,
            ShortVoteCount = current.ShortDirectionalVoteCount,
            HoldVoteCount = current.HoldVoteCount,
            HighRiskVoteCount = current.HighRiskVoteCount,
            LowTradeabilityVoteCount = current.LowTradeabilityVoteCount,
            ValidModelCount = current.ValidModelCount,
            TotalModelCount = current.TotalModelCount,
            ConsensusRiskLevel = current.RiskLevel,
            ConsensusTradeability = current.Tradeability,
            Stability = stability,
            BlockReason = blockReason,
            SupportingModels = current.DirectionalSupportingModels,
            ExecutableSupportingModels = current.ExecutableSupportingModels,
            OpposingModels = current.OpposingModels,
            HoldModels = current.HoldModels,
            ValidDecisions = current.ValidDecisions,
            BatchConsensuses = batches
        };
    }

    private string ResolveBatchBlockReason(
        string direction,
        decimal weightedScore,
        decimal directionalAgreement,
        int executableVoteCount,
        int highRiskVotes,
        int lowTradeabilityVotes)
    {
        if (!IsDirectional(direction))
            return "Batch consensus direction is neutral.";

        if (Math.Abs(weightedScore) < _options.MinBatchDirectionalScore)
        {
            return $"Batch directional score below threshold. Score={Math.Round(weightedScore, 2)}, Required={_options.MinBatchDirectionalScore}.";
        }

        if (directionalAgreement < _options.MinDirectionalAgreementRatio)
        {
            return $"Batch directional agreement below threshold. Agreement={Math.Round(directionalAgreement, 4)}, Required={_options.MinDirectionalAgreementRatio}.";
        }

        if (executableVoteCount < _options.MinExecutableVotes)
        {
            return $"Not enough executable votes in the selected direction. Votes={executableVoteCount}, Required={_options.MinExecutableVotes}.";
        }

        if (_options.BlockHighRisk && highRiskVotes > 0)
            return $"At least one model reported HIGH risk. HighRiskVotes={highRiskVotes}.";

        if (_options.BlockLowTradeability && lowTradeabilityVotes > 0)
        {
            return $"At least one model reported LOW tradeability. LowTradeabilityVotes={lowTradeabilityVotes}.";
        }

        return string.Empty;
    }

    private string ResolveTemporalBlockReason(
        QuantModelBatchConsensusDecision current,
        QuantModelBatchConsensusDecision? previous,
        QuantModelBatchConsensusDecision? third,
        bool threeDirectionsAgree,
        decimal temporalScore,
        decimal temporalSpanMinutes,
        int executableBatchCount)
    {
        if (previous is null || third is null)
            return $"Three completed Quant batches are required. Available={1 + (previous is null ? 0 : 1) + (third is null ? 0 : 1)}.";

        if (temporalSpanMinutes > _options.MaxThreeBatchSpanMinutes)
        {
            return $"Three-batch temporal span exceeds limit. SpanMinutes={Math.Round(temporalSpanMinutes, 2)}, Maximum={_options.MaxThreeBatchSpanMinutes}.";
        }

        if (_options.RequireThreeMatchingDirections && !threeDirectionsAgree)
        {
            return $"Three batch directions do not agree. Current={current.Direction}, Previous={previous.Direction}, Third={third.Direction}.";
        }

        if (!current.ShouldTrade)
            return $"Current batch is not executable. Reason={current.BlockReason}";

        if (executableBatchCount < _options.MinExecutableBatchCount)
        {
            return $"Not enough executable batches. Executable={executableBatchCount}, Required={_options.MinExecutableBatchCount}.";
        }

        if (Math.Abs(temporalScore) < _options.MinTemporalDirectionalScore)
        {
            return $"Temporal directional score below threshold. Score={Math.Round(temporalScore, 2)}, Required={_options.MinTemporalDirectionalScore}.";
        }

        return string.Empty;
    }

    private decimal CalculateTemporalScore(
        QuantModelBatchConsensusDecision current,
        QuantModelBatchConsensusDecision previous,
        QuantModelBatchConsensusDecision third)
    {
        var currentWeight = Math.Max(0m, _options.CurrentBatchWeight);
        var previousWeight = Math.Max(0m, _options.PreviousBatchWeight);
        var thirdWeight = Math.Max(0m, _options.ThirdBatchWeight);
        var totalWeight = currentWeight + previousWeight + thirdWeight;

        if (totalWeight <= 0)
            return 0m;

        return (
            current.WeightedDirectionalScore * currentWeight +
            previous.WeightedDirectionalScore * previousWeight +
            third.WeightedDirectionalScore * thirdWeight) / totalWeight;
    }

    private bool IsValidDecision(QuantModelDecision decision)
    {
        if (!decision.IsSuccess)
            return false;

        if (decision.LongConfidence is < 0 or > 100)
            return false;

        if (decision.ShortConfidence is < 0 or > 100)
            return false;

        if (decision.LongConfidence + decision.ShortConfidence != 100)
            return false;

        var expectedScore = decision.LongConfidence - decision.ShortConfidence;

        if (decision.DirectionalScore != expectedScore)
            return false;

        if (decision.IsLong && decision.DirectionalScore <= 0)
            return false;

        if (decision.IsShort && decision.DirectionalScore >= 0)
            return false;

        return decision.IsLong || decision.IsShort;
    }

    private decimal CalculateWeightedDirectionalScore(
        IReadOnlyList<QuantModelDecision> decisions)
    {
        decimal weightedSum = 0;
        decimal totalWeight = 0;

        foreach (var decision in decisions)
        {
            var weight = ResolveModelWeight(decision);

            weightedSum += decision.DirectionalScore * weight;
            totalWeight += weight;
        }

        return totalWeight <= 0
            ? 0
            : weightedSum / totalWeight;
    }

    private decimal ResolveModelWeight(QuantModelDecision decision)
    {
        var key = $"{decision.Provider} {decision.ModelName}".Trim();

        foreach (var item in _options.ModelWeights)
        {
            if (key.Contains(item.Key, StringComparison.OrdinalIgnoreCase))
                return item.Value <= 0 ? 1m : item.Value;
        }

        return 1m;
    }

    private static decimal CalculateDirectionalAgreement(
        IReadOnlyList<QuantModelDecision> decisions,
        string direction)
    {
        if (decisions.Count == 0 || !IsDirectional(direction))
            return 0m;

        var agreeing = decisions.Count(x => SupportsDirection(x, direction));
        return agreeing / (decimal)decisions.Count;
    }

    private static string ResolveDirection(decimal weightedScore)
    {
        if (weightedScore > 0)
            return "LONG";

        if (weightedScore < 0)
            return "SHORT";

        return "NEUTRAL";
    }

    private static bool IsDirectional(string direction) =>
        direction.Equals("LONG", StringComparison.OrdinalIgnoreCase) ||
        direction.Equals("SHORT", StringComparison.OrdinalIgnoreCase);

    private static bool SupportsDirection(
        QuantModelDecision decision,
        string direction)
    {
        return direction switch
        {
            "LONG" => decision.IsLong,
            "SHORT" => decision.IsShort,
            _ => false
        };
    }

    private static bool OpposesDirection(
        QuantModelDecision decision,
        string direction)
    {
        return direction switch
        {
            "LONG" => decision.IsShort,
            "SHORT" => decision.IsLong,
            _ => false
        };
    }

    private static bool IsHoldDecision(QuantModelDecision decision) =>
        !decision.ShouldTrade ||
        decision.RecommendedAction.Equals("HOLD", StringComparison.OrdinalIgnoreCase);

    private static bool IsExecutableVoteForDirection(
        QuantModelDecision decision,
        string direction)
    {
        if (!decision.ShouldTrade)
            return false;

        if (decision.RiskLevel.Equals("HIGH", StringComparison.OrdinalIgnoreCase))
            return false;

        if (decision.Tradeability.Equals("LOW", StringComparison.OrdinalIgnoreCase))
            return false;

        return direction switch
        {
            "LONG" =>
                decision.IsLong &&
                decision.RecommendedAction.Equals(
                    "ENTER_LONG",
                    StringComparison.OrdinalIgnoreCase),

            "SHORT" =>
                decision.IsShort &&
                decision.RecommendedAction.Equals(
                    "ENTER_SHORT",
                    StringComparison.OrdinalIgnoreCase),

            _ => false
        };
    }

    private static string ResolveConsensusRisk(
        IReadOnlyList<QuantModelDecision> decisions)
    {
        if (decisions.Any(x =>
            x.RiskLevel.Equals("HIGH", StringComparison.OrdinalIgnoreCase)))
        {
            return "HIGH";
        }

        if (decisions.Any(x =>
            x.RiskLevel.Equals("MEDIUM", StringComparison.OrdinalIgnoreCase)))
        {
            return "MEDIUM";
        }

        return "LOW";
    }

    private static string ResolveConsensusTradeability(
        IReadOnlyList<QuantModelDecision> decisions)
    {
        if (decisions.Any(x =>
            x.Tradeability.Equals("LOW", StringComparison.OrdinalIgnoreCase)))
        {
            return "LOW";
        }

        if (decisions.Any(x =>
            x.Tradeability.Equals("MEDIUM", StringComparison.OrdinalIgnoreCase)))
        {
            return "MEDIUM";
        }

        return "HIGH";
    }

    private string ResolveTemporalStability(
        bool hasThreeBatches,
        bool threeDirectionsAgree,
        decimal temporalSpanMinutes)
    {
        if (!hasThreeBatches)
            return "INSUFFICIENT_HISTORY";

        if (temporalSpanMinutes > _options.MaxThreeBatchSpanMinutes)
            return "THREE_BATCH_SPAN_EXCEEDED";

        return threeDirectionsAgree
            ? "THREE_BATCH_STABLE"
            : "THREE_BATCH_DIRECTION_MISMATCH";
    }
}
