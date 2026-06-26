namespace KinetixFlowEngine.Core.Database.Entities;

public sealed class MarketStateEntity
{
    public Guid Id { get; set; }

    public DateTime TimestampUtc { get; set; }

    public int Sequence { get; set; }

    public int SchemaVersion { get; set; }

    public short Timeframe { get; set; }

    public int EngineBuild { get; set; }

    public byte QualityScore { get; set; }

    public short Regime { get; set; }

    public string Payload { get; set; } = string.Empty;
}