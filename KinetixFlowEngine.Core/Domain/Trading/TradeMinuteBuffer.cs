using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Domain.Trading
{
    public sealed class TradeMinuteBuffer
    {
        private decimal _buyVolume;
        private decimal _sellVolume;

        private int _buyTrades;
        private int _sellTrades;

        private decimal _buyPriceVolume;
        private decimal _sellPriceVolume;

        private decimal _largestBuyTrade;
        private decimal _largestSellTrade;

        public void AddTrade(FlowTrade trade)
        {
            if (!trade.IsBuyerMaker)
            {
                _buyVolume += trade.Quantity;
                _buyTrades++;
                _buyPriceVolume += trade.Price * trade.Quantity;

                if (trade.Quantity > _largestBuyTrade)
                    _largestBuyTrade = trade.Quantity;
            }
            else
            {
                _sellVolume += trade.Quantity;
                _sellTrades++;
                _sellPriceVolume += trade.Price * trade.Quantity;

                if (trade.Quantity > _largestSellTrade)
                    _largestSellTrade = trade.Quantity;
            }
        }

        public TradeMinuteSnapshot CompleteMinute(DateTime minuteUtc)
        {
            var snapshot = new TradeMinuteSnapshot
            {
                MinuteUtc = minuteUtc,
                BuyVolume = _buyVolume,
                SellVolume = _sellVolume,
                BuyTrades = _buyTrades,
                SellTrades = _sellTrades,
                AverageBuyTradeSize = _buyTrades == 0 ? 0 : _buyVolume / _buyTrades,
                AverageSellTradeSize = _sellTrades == 0 ? 0 : _sellVolume / _sellTrades,
                BuyVWAP = _buyVolume == 0 ? 0 : _buyPriceVolume / _buyVolume,
                SellVWAP = _sellVolume == 0 ? 0 : _sellPriceVolume / _sellVolume,
                LargestBuyTrade = _largestBuyTrade,
                LargestSellTrade = _largestSellTrade
            };

            Reset();

            return snapshot;
        }

        public void Reset()
        {
            _buyVolume = 0;
            _sellVolume = 0;

            _buyTrades = 0;
            _sellTrades = 0;

            _buyPriceVolume = 0;
            _sellPriceVolume = 0;

            _largestBuyTrade = 0;
            _largestSellTrade = 0;
        }
    }
}
