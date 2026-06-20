using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Database.Repositories
{
    public interface IModelReviewRepository
    {
        Task<long> SaveAsync(
            ModelReviewEntity review,
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
    }
}
