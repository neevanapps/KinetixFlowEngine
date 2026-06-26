using KinetixFlowEngine.Core.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinetixFlowEngine.Core.Database.Configurations;

public sealed class MarketStateConfiguration
    : IEntityTypeConfiguration<MarketStateEntity>
{
    public void Configure(
        EntityTypeBuilder<MarketStateEntity> builder)
    {
        builder.ToTable("market_state");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TimestampUtc)
            .IsRequired();

        builder.Property(x => x.Sequence)
            .IsRequired();

        builder.Property(x => x.EngineBuild)
            .IsRequired();

        builder.Property(x => x.Timeframe)
            .IsRequired();

        builder.Property(x => x.QualityScore)
            .IsRequired();

        builder.Property(x => x.Regime)
            .IsRequired();

        builder.Property(x => x.SchemaVersion)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(x => x.TimestampUtc);

        builder.HasIndex(x =>
            new
            {
                x.Timeframe,
                x.TimestampUtc
            });

        builder.HasIndex(x => x.Sequence);
    }
}