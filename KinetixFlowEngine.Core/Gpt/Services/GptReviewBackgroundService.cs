using KinetixFlowEngine.Core.Database.Mappers;
using KinetixFlowEngine.Core.Database.Repositories;
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
    private readonly CompositeReviewService _reviewService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<GptReviewBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public GptReviewBackgroundService(
        GptReviewQueue queue,
        CompositeReviewService reviewService,
        INotificationService notificationService,
        ILogger<GptReviewBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _queue = queue;
        _reviewService = reviewService;
        _notificationService = notificationService;
        _logger = logger;
        _scopeFactory = scopeFactory;
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

                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISnapshotRepository>();
                var snapshotEntity = SnapshotMapper.Map(snapshot);
                long snapshotId = await repository.SaveSnapshotAsync(snapshotEntity, stoppingToken);
                _logger.LogInformation("Snapshot Saved | Seq:{Seq} | DbId:{Id}", snapshot.Sequence, snapshotId);

                var stopwatch = Stopwatch.StartNew();
                var reviews =
                    await _reviewService.ReviewAllAsync(
                        snapshot,
                        stoppingToken);
                stopwatch.Stop();

                foreach (var review in reviews)
                {
                    await _notificationService.SendGroupMessageAsync(
                        BuildTelegramMessage(
                            review,
                            snapshot.Price,
                            stopwatch.Elapsed));
                }
                using var reviewScope = _scopeFactory.CreateScope();
                var reviewRepo = reviewScope.ServiceProvider.GetRequiredService<IModelReviewRepository>();
                foreach (var review in reviews)
                {
                    var entity = ReviewMapper.Map(snapshotId, review);
                    await reviewRepo.SaveAsync(entity, stoppingToken);
                }

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
🤖 Model: {review.ModelName}

Price: {price:C}
Seq: {review.Sequence}
Time Taken: {timeTaken}

Bias: {review.Assessment.DirectionalBias}
Direction: {review.Assessment.RecommendedAction}
DominantIntent: {review.Assessment.DominantIntent}
MarketStructure: {review.Assessment.MarketStructure}
Tradeability: {review.Assessment.Tradeability}

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

    private static string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalSeconds < 60)
            return $"{elapsed.TotalSeconds:F1}s";

        return $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s";
    }
}