using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class ReviewSmoothingService
{
    private readonly ILogger<ReviewSmoothingService> _logger;

    public ReviewSmoothingService(ILogger<ReviewSmoothingService> logger)
    {
        _logger = logger;
    }

    public SmoothedModelReview Calculate(
        string modelName,
        IReadOnlyList<GptReviewRecord> reviews)
    {
        if (reviews.Count == 0)
            throw new InvalidOperationException();

        var ordered =
            reviews.OrderBy(x => x.Sequence)
                   .ToList();

        double[] weights = ordered.Count switch
        {
            1 => new double[] { 1.0 },
            2 => new double[] { 0.4, 0.6 },
            _ => new double[] { 0.2, 0.3, 0.5 }
        };

        double score = 0;
        double longConf = 0;
        double shortConf = 0;

        for (int i = 0; i < ordered.Count; i++)
        {
            score +=
                ordered[i].Assessment.Score *
                weights[i];

            longConf +=
                ordered[i].Assessment.LongConfidence *
                weights[i];

            shortConf +=
                ordered[i].Assessment.ShortConfidence *
                weights[i];
        }

        double velocity = 0;

        if (ordered.Count >= 2)
        {
            velocity =
                ordered.Last().Assessment.Score -
                ordered.First().Assessment.Score;
        }

        
        return new SmoothedModelReview
        {
            ModelName = modelName,

            Score = score,

            LongConfidence = longConf,

            ShortConfidence = shortConf,

            ScoreVelocity = velocity,

            LatestSequence =
                ordered.Last().Sequence,

            Direction =
                longConf >= shortConf
                    ? "Long"
                    : "Short"
        };
    }
}