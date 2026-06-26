using KinetixFlowEngine.Core.Database.Entities;
using KinetixFlowEngine.Core.Database.Serialization;
using KinetixFlowEngine.Core.Domain.Market;
using Microsoft.EntityFrameworkCore;

namespace KinetixFlowEngine.Core.Database.Repositories
{
    public interface IMarketStateRepository
    {
        Task SaveAsync(
            MarketState state,
            CancellationToken cancellationToken = default);

        Task<MarketState?> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<MarketState>> GetLatestAsync(
            MarketTimeframe timeframe,
            int count,
            CancellationToken cancellationToken = default);
    }

    public sealed class MarketStateRepository
    : IMarketStateRepository
    {
        private readonly KinetixDbContext _db;

        private readonly IJsonSerializer<MarketState> _serializer;

        public MarketStateRepository(
            KinetixDbContext db,
            IJsonSerializer<MarketState> serializer)
        {
            _db = db;
            _serializer = serializer;
        }

        public async Task SaveAsync(
            MarketState state,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(state);

            var entity = ToEntity(state);

            await _db.MarketStates.AddAsync(
                entity,
                cancellationToken);

            await _db.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<MarketState?> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity =
                await _db.MarketStates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id,
                        cancellationToken);

            if (entity is null)
                return null;

            return _serializer.Deserialize(entity.Payload);
        }

        public async Task<IReadOnlyList<MarketState>> GetLatestAsync(
            MarketTimeframe timeframe,
            int count,
            CancellationToken cancellationToken = default)
        {
            var entities =
                await _db.MarketStates
                    .AsNoTracking()
                    .Where(x => x.Timeframe == (short)timeframe)
                    .OrderByDescending(x => x.TimestampUtc)
                    .Take(count)
                    .ToListAsync(cancellationToken);

            return entities
                .Select(x => _serializer.Deserialize(x.Payload))
                .ToList();
        }

        private MarketStateEntity ToEntity(
            MarketState state)
        {
            return new MarketStateEntity
            {
                Id = state.Id,

                TimestampUtc = state.TimestampUtc,

                Sequence = state.Sequence,

                EngineBuild = state.EngineBuild,

                SchemaVersion = 1,

                Timeframe = (short)state.Timeframe,

                QualityScore = state.QualityScore,

                Regime = (short)state.Regime,

                Payload = _serializer.Serialize(state)
            };
        }
    }
}
