using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace KinetixFlowEngine.Core.Database
{
    public class KinetixDbContext : DbContext
    {
        public KinetixDbContext(
        DbContextOptions<KinetixDbContext> options)
        : base(options)
        {
        }


        public DbSet<MarketSnapshotEntity> MarketSnapshots => Set<MarketSnapshotEntity>();

        public DbSet<ModelReviewEntity> ModelReviews => Set<ModelReviewEntity>();

        public DbSet<MarketOutcomeEntity> MarketOutcomes => Set<MarketOutcomeEntity>();

        public DbSet<MarketPriceEntity> MarketPrices => Set<MarketPriceEntity>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ModelReviewEntity>()
                .HasOne(x => x.Snapshot)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.SnapshotId);

            builder.Entity<MarketOutcomeEntity>()
                .HasOne(x => x.Snapshot)
                .WithMany()
                .HasForeignKey(x => x.SnapshotId);

            builder.Entity<MarketPriceEntity>(entity =>
            {
                entity.ToTable("MarketPrices");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Price)
                    .HasPrecision(18, 8);

                entity.HasIndex(x => x.TimestampUtc);
            });
        }
    }
}
