using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Flow
{
    public class WhaleClusterSnapshot
    {
        public int LargeBuyTrades { get; set; }
        public int LargeSellTrades { get; set; }

        public double BuyClusterStrength { get; set; }
        public double SellClusterStrength { get; set; }

        public bool BullishWhaleActivity => BuyClusterStrength > SellClusterStrength;
        public bool BearishWhaleActivity => SellClusterStrength > BuyClusterStrength;
    }

    public class WhaleClusterEngine
    {
        // multiplier for whale definition
        private const decimal WhaleMultiplier = 5m;

        public WhaleClusterSnapshot Detect(IEnumerable<FlowTrade> trades, long cutoffTimestamp)
        {
            int buyLarge = 0;
            int sellLarge = 0;

            decimal buyVolume = 0;
            decimal sellVolume = 0;

            decimal totalVolume = 0;
            int tradeCount = 0;

            //------------------------------------------------
            // First pass: calculate average trade size
            //------------------------------------------------

            foreach (var trade in trades)
            {
                if (trade.Timestamp < cutoffTimestamp)
                    continue;

                totalVolume += trade.Quantity;
                tradeCount++;
            }

            if (tradeCount == 0)
            {
                return new WhaleClusterSnapshot();
            }

            decimal avgTradeSize = totalVolume / tradeCount;

            decimal whaleThreshold = avgTradeSize * WhaleMultiplier;

            //------------------------------------------------
            // Second pass: detect whale trades
            //------------------------------------------------

            foreach (var trade in trades)
            {
                if (trade.Timestamp < cutoffTimestamp)
                    continue;

                if (trade.Quantity < whaleThreshold)
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