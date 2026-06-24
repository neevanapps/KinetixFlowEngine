using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class ConsensusReviewService
{
    public ConsensusReview Calculate(
        IReadOnlyList<SmoothedModelReview> reviews)
    {
        if (reviews.Count == 0)
            throw new InvalidOperationException();

        double totalWeight = 0;

        double score = 0;
        double longConf = 0;
        double shortConf = 0;
        double velocity = 0;

        foreach (var review in reviews)
        {
            double weight =
                review.ModelName switch
                {
                    "mistral-small:latest" => 0.40,
                    "gpt-oss-120b" => 0.35,
                    "zai-glm-4.7" => 0.25,
                    _ => 0.20
                };

            totalWeight += weight;

            score += review.Score * weight;

            longConf +=
                review.LongConfidence * weight;

            shortConf +=
                review.ShortConfidence * weight;

            velocity +=
                review.ScoreVelocity * weight;
        }

        score /= totalWeight;
        longConf /= totalWeight;
        shortConf /= totalWeight;
        velocity /= totalWeight;

        return new ConsensusReview
        {
            Score = score,

            LongConfidence = longConf,

            ShortConfidence = shortConf,

            ScoreVelocity = velocity,

            Direction =
                longConf >= shortConf
                    ? "Long"
                    : "Short"
        };
    }
}