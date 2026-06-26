using KinetixFlowEngine.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Depth
{
    public sealed class DepthWallState
    {
        public decimal BidWallPrice { get; init; }

        public decimal BidWallQuantity { get; init; }

        public decimal AskWallPrice { get; init; }

        public decimal AskWallQuantity { get; init; }

        public int BidWallAgeSeconds { get; init; }

        public int AskWallAgeSeconds { get; init; }

        public bool BidWallConsumed { get; init; }

        public bool AskWallConsumed { get; init; }

        public MetricBehaviour BidBehaviour { get; init; }

        public MetricBehaviour AskBehaviour { get; init; }
    }

    public sealed class DepthWall
    {
        public decimal Price { get; set; }

        public decimal InitialQuantity { get; set; }

        public decimal CurrentQuantity { get; set; }

        public DateTime FirstSeenUtc { get; set; }

        public DateTime LastSeenUtc { get; set; }

        public bool IsConsumed { get; set; }

        public double QuantityChangePercent { get; set; }

        public decimal MaxQuantitySeen { get; set; }
    }
}
