using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Depth
{
    public sealed class DepthSecondFeature
    {
        public DateTime TimestampUtc { get; init; }

        public double ImbalanceTop5 { get; init; }

        public double ImbalanceTop10 { get; init; }

        public double LargestBidStrength { get; init; }

        public double LargestAskStrength { get; init; }
        public decimal Price { get; init; }
    }
}
