using KinetixFlowEngine.Core.Gpt.Models;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Database.Mappers;

public static class ReviewMapper
{
    public static ModelReviewEntity Map(
        long snapshotId,
        GptReviewRecord review)
    {
        return new ModelReviewEntity
        {
            SnapshotId = snapshotId,
            ModelName = review.ModelName,
            Sequence = review.Sequence,
            CreatedUtc = DateTime.UtcNow,
            DirectionalBias = review.Assessment.DirectionalBias,
            RecommendedAction = review.Assessment.RecommendedAction,
            LongConfidence = review.Assessment.LongConfidence,
            ShortConfidence = review.Assessment.ShortConfidence,
            Score = (int)review.Assessment.Score,
            TrendQuality = review.Assessment.TrendQuality,
            FlowQuality = review.Assessment.FlowQuality,
            RegimeQuality = review.Assessment.RegimeQuality,
            RiskLevel = review.Assessment.RiskLevel,
            Tradeability = review.Assessment.Tradeability,
            BehaviorEvidenceJson = JsonSerializer.Serialize(review.Assessment.BehaviorEvidence),
            KeyDriversJson = JsonSerializer.Serialize(review.Assessment.KeyDrivers),
            ContradictionsJson = JsonSerializer.Serialize(review.Assessment.Contradictions),
            DominantIntent = review.Assessment.DominantIntent,
            MarketStructure = review.Assessment.MarketStructure,
            StateAssessment = review.Assessment.StateAssessment,
            Summary = review.Assessment.Summary,
            RawResponse = review.RawResponse
        };
    }
}