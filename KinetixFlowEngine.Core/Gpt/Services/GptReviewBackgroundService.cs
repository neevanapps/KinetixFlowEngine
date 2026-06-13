using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class GptReviewBackgroundService
    : BackgroundService
{
    private readonly GptReviewQueue _queue;
    private readonly IGptReviewService _reviewService;
    private readonly TelegramService _telegram;
    private readonly ILogger<GptReviewBackgroundService> _logger;
    public GptReviewBackgroundService(
        GptReviewQueue queue,
        IGptReviewService reviewService,
        TelegramService telegram,
        ILogger<GptReviewBackgroundService> logger)
    {
        _queue = queue;
        _reviewService = reviewService;
        _telegram = telegram;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await foreach (var snapshot in
            _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation(
                    "Processing GPT Review | Seq:{Seq}",
                    snapshot.Sequence);

                var review =
                    await _reviewService.ReviewAsync(
                        snapshot,
                        stoppingToken);

                await _telegram.SendMessageAsync(
                    BuildTelegramMessage(review, snapshot.Price));

                _logger.LogInformation(
                    "GPT Review Completed | Seq:{Seq}",
                    snapshot.Sequence);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "GPT Review Failed");
            }
        }
    }

    private static string BuildTelegramMessage(
        GptReviewRecord review, decimal price)
    {
        return
$"""
🤖 GPT REVIEW

Price: {price:C}
Seq: {review.Sequence}

Bias: {review.Assessment.DirectionalBias}

Long: {review.Assessment.LongConfidence}%
Short: {review.Assessment.ShortConfidence}%

Score: {review.Assessment.Score}
Risk: {review.Assessment.RiskLevel}
State: {review.Assessment.StateAssessment}

Trend: {review.Assessment.TrendQuality}
Flow: {review.Assessment.FlowQuality}
Regime: {review.Assessment.RegimeQuality}

Summary:
{review.Assessment.Summary}
""";
    }
}