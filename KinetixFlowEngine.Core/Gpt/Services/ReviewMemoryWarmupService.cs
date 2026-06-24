using KinetixFlowEngine.Core.Database.Mappers;
using KinetixFlowEngine.Core.Database.Repositories;
using KinetixFlowEngine.Core.Gpt.Models;
using Microsoft.Extensions.Hosting;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class ReviewMemoryWarmupService
    : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LlmReviewMemory _memory;
    private readonly ILogger<ReviewMemoryWarmupService> _logger;

    public ReviewMemoryWarmupService(
        IServiceScopeFactory scopeFactory,
        LlmReviewMemory memory,
        ILogger<ReviewMemoryWarmupService> logger)
    {
        _scopeFactory = scopeFactory;
        _memory = memory;
        _logger = logger;
    }

    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var repository =
            scope.ServiceProvider
                 .GetRequiredService<IModelReviewRepository>();

        var reviews =
            await repository.GetRecentReviewsAsync(
                3,
                cancellationToken);

        foreach (var entity in reviews)
        {
            var review =
                ReviewEntityMapper.ToReview(entity);

            _memory.Update(review);
        }

        _logger.LogInformation(
            "Review Memory Warmed Up | Reviews:{Count}",
            reviews.Count);
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}