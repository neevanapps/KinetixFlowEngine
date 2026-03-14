using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Flow
{
    public class WhaleClusterSnapshot
    {
        public long Timestamp { get; set; }
        public int LargeBuyTrades { get; set; }
        public int LargeSellTrades { get; set; }

        public double BuyClusterStrength { get; set; }
        public double SellClusterStrength { get; set; }

        public bool BullishWhaleActivity => BuyClusterStrength > SellClusterStrength;
        public bool BearishWhaleActivity => SellClusterStrength > BuyClusterStrength;
    }

    public class WhaleClusterEngine
    {
        private const decimal WhaleThreshold = 3m; // BTC size threshold

        public WhaleClusterSnapshot Detect(IEnumerable<FlowTrade> trades, long cutoffTimestamp)
        {
            int buyLarge = 0;
            int sellLarge = 0;

            decimal buyVolume = 0;
            decimal sellVolume = 0;

            foreach (var trade in trades)
            {
                if (trade.Timestamp < cutoffTimestamp)
                    continue;

                if (trade.Quantity < WhaleThreshold)
                    continue;

                if (!trade.IsBuyerMaker)
                {
                    buyLarge++;
                    buyVolume += trade.Quantity;
                }
                else
                {
                    sellLarge++;
                    sellVolume += trade.Quantity;
                }
            }

            double buyStrength = buyLarge == 0 ? 0 : (double)(buyVolume / buyLarge);
            double sellStrength = sellLarge == 0 ? 0 : (double)(sellVolume / sellLarge);

            return new WhaleClusterSnapshot
            {
                LargeBuyTrades = buyLarge,
                LargeSellTrades = sellLarge,
                BuyClusterStrength = buyStrength,
                SellClusterStrength = sellStrength
            };
        }
    }
}