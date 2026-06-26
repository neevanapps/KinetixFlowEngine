using KinetixFlowEngine.Core.Domain.Common;

namespace KinetixFlowEngine.Core.Domain.Trading;

public sealed class TradeVolume
{
    public MetricState Buy { get; set; } = new();

    public MetricState Sell { get; set; } = new();

    public MetricState Delta { get; set; } = new();
}

public sealed class TradeCount
{
    public MetricState Buy { get; set; } = new();

    public MetricState Sell { get; set; } = new();
}

public sealed class TradeSize
{
    public MetricState Buy { get; set; } = new();

    public MetricState Sell { get; set; } = new();

    public MetricState LargestBuy { get; set; } = new();

    public MetricState LargestSell { get; set; } = new();
}

public sealed class TradeExecution
{
    public MetricState BuyVWAP { get; set; } = new();

    public MetricState SellVWAP { get; set; } = new();
}

public sealed class Trade : IMarketDomain
{
    public TradeVolume Volume { get; set; } = new();

    public TradeCount Trades { get; set; } = new();

    public TradeSize TradeSize { get; set; } = new();

    public TradeExecution Execution { get; set; } = new();

    public DomainSummary Summary { get; set; } = new();

    public IReadOnlyList<MarketEvent> Events { get; set; }
        = Array.Empty<MarketEvent>();
}