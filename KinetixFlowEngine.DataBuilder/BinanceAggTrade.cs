using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.DataBuilder
{
    public class BinanceAggTrade
    {
        public long a { get; set; }      // aggTradeId
        public string p { get; set; }    // price
        public string q { get; set; }    // quantity
        public long T { get; set; }      // timestamp
        public bool m { get; set; }      // buyer maker
    }
}
