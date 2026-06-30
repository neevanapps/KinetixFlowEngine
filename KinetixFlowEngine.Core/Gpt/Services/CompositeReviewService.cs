using KinetixFlowEngine.Core.Gpt.Models;
using System.Runtime.CompilerServices;

namespace KinetixFlowEngine.Core.Gpt.Services;

public sealed class CompositeReviewService
{
    private readonly IEnumerable<ILocalModelReviewer>
        _localReviewers;

    private readonly IEnumerable<ICloudModelReviewer>
        _cloudReviewers;

    private readonly ILogger _logger;

    public CompositeReviewService(
        IEnumerable<ILocalModelReviewer> localReviewers,
        IEnumerable<ICloudModelReviewer> cloudReviewers,
        ILogger<CompositeReviewService> logger)
    {
        _localReviewers = localReviewers;
        _cloudReviewers = cloudReviewers;
        _logger = logger;
    }

    public async Task<List<GptReviewRecord>>
        ReviewAllAsync(
            GptMarketSnapshotV2 snapshot,
            CancellationToken ct = default)
    {
        var results =
            new List<GptReviewRecord>();

        //
        // Start cloud immediately
        //

        //var cloudTasks =
        //_cloudReviewers
        //    .Select(async reviewer =>
        //    {
        //        try
        //        {
        //            return await reviewer.ReviewAsync(
        //                snapshot,
        //                ct);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(
        //                ex,
        //                "Cloud reviewer failed: {Reviewer}",
        //                reviewer.Name);

        //            return null;
        //        }
        //    })
        //    .ToList();

        //
        // Run local sequentially
        //

        foreach (var reviewer in _localReviewers)
        {
            try
            {
                var review =
                    await reviewer.ReviewAsync(
                        snapshot,
                        ct);

                if (review != null)
                {
                    results.Add(review);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Local reviewer failed: {Reviewer}",
                    reviewer.Name);
            }
        }

        //
        // Await cloud completion
        //

        //var cloudResults =
        //    await Task.WhenAll(cloudTasks);

        //results.AddRange(cloudResults.Where(x => x != null)!);

        return results;
    }
}