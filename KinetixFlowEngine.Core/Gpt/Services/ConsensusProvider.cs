using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class ConsensusProvider
{
    private readonly LlmReviewMemory _memory;
    private readonly ReviewSmoothingService _smoothing;
    private readonly ConsensusReviewService _consensus;

    public ConsensusProvider(
        LlmReviewMemory memory,
        ReviewSmoothingService smoothing,
        ConsensusReviewService consensus)
    {
        _memory = memory;
        _smoothing = smoothing;
        _consensus = consensus;
    }

    public ConsensusReview? GetConsensus()
    {
        var smoothed =
            new List<SmoothedModelReview>();

        foreach (var kv in _memory.GetAllReviews())
        {
            if (kv.Value.Count == 0)
                continue;

            smoothed.Add(
                _smoothing.Calculate(
                    kv.Key,
                    kv.Value));
        }

        if (smoothed.Count == 0)
            return null;

        return _consensus.Calculate(
            smoothed);
    }
}