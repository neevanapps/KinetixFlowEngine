namespace KinetixFlowEngine.Core.Flow
{
    public class FlowWindowSnapshot
    {
        public decimal BuyVolume { get; set; }

        public decimal SellVolume { get; set; }

        public int BuyTrades { get; set; }

        public int SellTrades { get; set; }

        public int TradeCount => BuyTrades + SellTrades;

        public decimal TotalVolume => BuyVolume + SellVolume;
    }
}