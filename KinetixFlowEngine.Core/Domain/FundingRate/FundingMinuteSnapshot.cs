using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Domain.FundingRate
{
    public sealed class FundingMinuteSnapshot
    {
        public DateTime MinuteUtc { get; init; }

        public IReadOnlyList<FundingObservation> Snapshots { get; init; }
            = Array.Empty<FundingObservation>();

        public int Count => Snapshots.Count;
    }
}
