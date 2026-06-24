using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class LlmReviewMemory
{
    private readonly object _lock = new();

    private readonly Dictionary<string, Queue<GptReviewRecord>>
        _reviews = new();

    private const int MaxHistory = 3;

    public void Update(GptReviewRecord review)
    {
        lock (_lock)
        {
            if (!_reviews.TryGetValue(
                review.ModelName,
                out var queue))
            {
                queue = new Queue<GptReviewRecord>();

                _reviews[review.ModelName] = queue;
            }

            queue.Enqueue(review);

            while (queue.Count > MaxHistory)
            {
                queue.Dequeue();
            }
        }
    }

    public IReadOnlyList<GptReviewRecord>
        GetReviews(string modelName)
    {
        lock (_lock)
        {
            if (!_reviews.TryGetValue(
                modelName,
                out var queue))
            {
                return [];
            }

            return queue.ToList();
        }
    }

    public IReadOnlyDictionary<
        string,
        IReadOnlyList<GptReviewRecord>>
        GetAll()
    {
        lock (_lock)
        {
            return _reviews.ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<GptReviewRecord>)
                    x.Value.ToList());
        }
    }

    public IReadOnlyDictionary<
    string,
    IReadOnlyList<GptReviewRecord>>
    GetAllReviews()
    {
        lock (_lock)
        {
            return _reviews.ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<GptReviewRecord>)
                    x.Value.ToList());
        }
    }


}