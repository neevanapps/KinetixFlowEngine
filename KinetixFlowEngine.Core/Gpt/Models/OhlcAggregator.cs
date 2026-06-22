using KinetixFlowEngine.Core.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Models
{
    public sealed class OhlcAggregator
    {
        public CandleSnapshot Build(
            IReadOnlyList<MarketPriceEntity> rows)
        {
            if (rows.Count == 0)
            {
                return new CandleSnapshot();
            }

            return new CandleSnapshot
            {
                Open = rows.First().Price,
                High = rows.Max(x => x.Price),
                Low = rows.Min(x => x.Price),
                Close = rows.Last().Price
            };
        }
    }
}
