using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public sealed class CompositeReviewService
    {
        private readonly IEnumerable<IModelReviewer> _reviewers;

        public CompositeReviewService(
            IEnumerable<IModelReviewer> reviewers)
        {
            _reviewers = reviewers;
        }

        public async Task<List<GptReviewRecord>> ReviewAllAsync(
            GptMarketSnapshotV2 snapshot,
            CancellationToken ct = default)
        {
            var results = new List<GptReviewRecord>();

            foreach (var reviewer in _reviewers)
            {
                var review =
                    await reviewer.ReviewAsync(snapshot, ct);

                results.Add(review);
            }

            return results;
        }
    }
}
