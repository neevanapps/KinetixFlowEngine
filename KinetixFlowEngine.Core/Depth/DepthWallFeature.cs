using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Depth
{
    public sealed class DepthWallFeature
    {
        public DateTime TimestampUtc { get; init; }

        public decimal LargestBidWallPrice { get; init; }

        public decimal LargestBidWallQty { get; init; }

        public decimal LargestAskWallPrice { get; init; }

        public decimal LargestAskWallQty { get; init; }

        public int BidWallPersistenceSeconds { get; init; }

        public int AskWallPersistenceSeconds { get; init; }

        public bool BidWallBeingConsumed { get; init; }

        public bool AskWallBeingConsumed { get; init; }
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
