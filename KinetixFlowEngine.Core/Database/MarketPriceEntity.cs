using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Database
{
    public sealed class MarketPriceEntity
    {
        public long Id { get; set; }

        public DateTime TimestampUtc { get; set; }

        public decimal Price { get; set; }
    }
}
