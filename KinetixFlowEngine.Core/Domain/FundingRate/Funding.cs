using KinetixFlowEngine.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Domain.FundingRate
{
    public sealed class Funding : IMarketDomain
    {
        //------------------------------------
        // Funding
        //------------------------------------

        public MetricState Rate { get; set; } = new();

        public MetricState Pressure { get; set; } = new();

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

    public sealed class FundingObservation
    {
        public DateTime TimestampUtc { get; init; }

        /// <summary>
        /// Raw funding rate returned by Bybit.
        /// Example: 0.0001
        /// </summary>
        public decimal Rate { get; init; }
    }
}
