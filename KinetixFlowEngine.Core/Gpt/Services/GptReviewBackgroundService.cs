using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class GptReviewBackgroundService
    : BackgroundService
{
    private readonly GptReviewQueue _queue;
    private readonly IGptReviewService _reviewService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<GptReviewBackgroundService> _logger;
    public GptReviewBackgroundService(
        GptReviewQueue queue,
        IGptReviewService reviewService,
        INotificationService notificationService,
        ILogger<GptReviewBackgroundService> logger)
    {
        _queue = queue;
        _reviewService = reviewService;
        _notificationService = notificationService;
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

                var stopwatch = Stopwatch.StartNew();
                var review =
                    await _reviewService.ReviewAsync(
                        snapshot,
                        stoppingToken);
                stopwatch.Stop();

                await _notificationService.SendGroupMessageAsync(
                    BuildTelegramMessage(review, snapshot.Price, stopwatch.Elapsed));

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
        GptReviewRecord review, decimal price, TimeSpan duration)
    {
        string timeTaken = FormatElapsedTime(duration);
        return
$"""
🤖 GPT REVIEW

Price: {price:C}
Seq: {review.Sequence}
Time Taken: {timeTaken}

Bias: {review.Assessment.DirectionalBias}

Long: {review.Assessment.LongConfidence}%
Short: {review.Assessment.ShortConfidence}%

DominantIntent: {review.Assessment.DominantIntent}
BehaviorEvidence: {string.Join(", ", review.Assessment.BehaviorEvidence)}

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

    private static string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalSeconds < 60)
            return $"{elapsed.TotalSeconds:F1}s";

        return $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s";
    }
}