using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Domain.Trading
{
    public sealed class TradeMinuteSnapshot
    {
        public DateTime MinuteUtc { get; init; }

        public decimal BuyVolume { get; init; }

        public decimal SellVolume { get; init; }

        public int BuyTrades { get; init; }

        public int SellTrades { get; init; }

        public decimal AverageBuyTradeSize { get; init; }

        public decimal AverageSellTradeSize { get; init; }

        public decimal LargestBuyTrade { get; init; }

        public decimal LargestSellTrade { get; init; }

        public decimal BuyVWAP { get; init; }

        public decimal SellVWAP { get; init; }

        public int TotalTrades => BuyTrades + SellTrades;
    }
}
