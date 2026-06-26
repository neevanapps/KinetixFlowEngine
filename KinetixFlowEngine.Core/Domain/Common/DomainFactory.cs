using KinetixFlowEngine.Core.Domain.Common;

public abstract class DomainFactory<T>
    where T : class, IMarketDomain
{
    protected T Complete(
        T domain,
        Func<T, DomainSummary> summaryBuilder,
        Func<T, IReadOnlyList<MarketEvent>> eventBuilder)
    {
        domain.Summary = summaryBuilder(domain);
        domain.Events = eventBuilder(domain);

        OnCompleted(domain);

        return domain;
    }

    protected virtual void OnCompleted(T domain)
    {
    }
}