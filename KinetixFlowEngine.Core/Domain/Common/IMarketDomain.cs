namespace KinetixFlowEngine.Core.Domain.Common;

public interface IMarketDomain
{
    DomainSummary Summary { get; set; }

    IReadOnlyList<MarketEvent> Events { get; set; }
}