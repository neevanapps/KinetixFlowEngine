using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class ModelReviewProvider
{
    private readonly LlmReviewMemory _memory;
    private readonly ReviewSmoothingService _smoothing;

    public ModelReviewProvider(
        LlmReviewMemory memory,
        ReviewSmoothingService smoothing)
    {
        _memory = memory;
        _smoothing = smoothing;
    }

    public SmoothedModelReview?
        GetModelReview(
            string modelName)
    {
        var reviews =
            _memory.GetReviews(modelName);

        if (reviews.Count == 0)
            return null;

        return _smoothing.Calculate(
            modelName,
            reviews);
    }
}