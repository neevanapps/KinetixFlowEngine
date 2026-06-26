using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Pricing;

public sealed class Price : IMarketDomain
{
    //------------------------------------
    // Candle
    //------------------------------------

    public MinuteCandle Candle { get; set; } = new();

    //------------------------------------
    // Market Reference
    //------------------------------------

    public decimal VWAP { get; set; }

    public decimal ATR { get; set; }

    //------------------------------------
    // Relative Position
    //------------------------------------

    public MetricState DistanceFromVWAP { get; set; } = new();

    public MetricState DistanceFromPreviousClose { get; set; } = new();

    //------------------------------------
    // Summary
    //------------------------------------

    public DomainSummary Summary { get; set; } = new();

    //------------------------------------
    // Events
    //------------------------------------

    public IReadOnlyList<MarketEvent> Events { get; set; }
        = Array.Empty<MarketEvent>();

}