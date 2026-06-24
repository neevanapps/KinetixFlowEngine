using KinetixFlowEngine.Core.Gpt.Models;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Database.Mappers;

public static class ReviewEntityMapper
{
    public static GptReviewRecord ToReview(
        ModelReviewEntity entity)
    {
        return new GptReviewRecord
        {
            CreatedUtc = entity.CreatedUtc,
            Sequence = entity.Sequence,
            ModelName = entity.ModelName,
            RawResponse = entity.RawResponse,
            Assessment =
                JsonSerializer.Deserialize<GptAssessment>(
                    entity.RawResponse)!
        };
    }
}