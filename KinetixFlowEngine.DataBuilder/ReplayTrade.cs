using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.DataBuilder
{
    public struct ReplayTrade
    {
        public long Timestamp;
        public double Price;
        public double Quantity;
        public bool IsBuyerMaker;
    }
}
