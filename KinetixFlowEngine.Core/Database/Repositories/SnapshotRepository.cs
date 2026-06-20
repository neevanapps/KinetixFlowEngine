using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Database.Repositories
{
    public interface ISnapshotRepository
    {
        Task<long> SaveSnapshotAsync(
            MarketSnapshotEntity snapshot,
            CancellationToken ct = default);
    }

    public sealed class SnapshotRepository
        : ISnapshotRepository
    {
        private readonly KinetixDbContext _db;

        public SnapshotRepository(KinetixDbContext db)
        {
            _db = db;
        }

        public async Task<long> SaveSnapshotAsync(
            MarketSnapshotEntity snapshot,
            CancellationToken ct = default)
        {
            _db.MarketSnapshots.Add(snapshot);

            await _db.SaveChangesAsync(ct);

            return snapshot.Id;
        }
    }
}
