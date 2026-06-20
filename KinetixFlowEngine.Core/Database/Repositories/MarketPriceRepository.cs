using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Database.Repositories
{
    public interface IMarketPriceRepository
    {
        Task SaveAsync(
            MarketPriceEntity entity,
            CancellationToken ct = default);
    }

    public sealed class MarketPriceRepository
    : IMarketPriceRepository
    {
        private readonly KinetixDbContext _db;

        public MarketPriceRepository(
            KinetixDbContext db)
        {
            _db = db;
        }

        public async Task SaveAsync(
            MarketPriceEntity entity,
            CancellationToken ct = default)
        {
            _db.MarketPrices.Add(entity);

            await _db.SaveChangesAsync(ct);
        }
    }
}
