using KinetixFlowEngine.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Domain.OI
{
    public sealed class OpenInterest : IMarketDomain
    {
        public MetricState Value { get; set; } = new();

        public MetricState Change { get; set; } = new();

        public DomainSummary Summary { get; set; } = new();

        public IReadOnlyList<MarketEvent> Events { get; set; }
            = Array.Empty<MarketEvent>();
    }

    public sealed class OpenInterestObservation
    {
        public DateTime TimestampUtc { get; init; }

        public decimal Value { get; init; }
    }
}
