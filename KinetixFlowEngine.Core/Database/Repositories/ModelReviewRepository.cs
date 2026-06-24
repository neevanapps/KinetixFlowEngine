using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;


namespace KinetixFlowEngine.Core.Database.Repositories
{
    public interface IModelReviewRepository
    {
        Task<long> SaveAsync(
            ModelReviewEntity review,
            CancellationToken ct = default);

        Task<List<ModelReviewEntity>> GetRecentReviewsAsync(
        int countPerModel,
        CancellationToken ct = default);
    }

    public sealed class ModelReviewRepository
        : IModelReviewRepository
    {
        private readonly KinetixDbContext _db;

        public ModelReviewRepository(
            KinetixDbContext db)
        {
            _db = db;
        }

        public async Task<long> SaveAsync(
            ModelReviewEntity review,
            CancellationToken ct = default)
        {
            _db.ModelReviews.Add(review);

            await _db.SaveChangesAsync(ct);

            return review.Id;
        }

        public async Task<List<ModelReviewEntity>> GetRecentReviewsAsync(
    int countPerModel,
    CancellationToken ct = default)
        {
            var allReviews =
                await _db.ModelReviews
                    .OrderByDescending(x => x.Sequence)
                    .ToListAsync(ct);

            return allReviews
                .GroupBy(x => x.ModelName)
                .SelectMany(x =>
                    x.OrderByDescending(y => y.Sequence)
                     .Take(countPerModel))
                .ToList();
        }
    }
}
