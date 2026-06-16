using KinetixFlowEngine.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Depth
{
    public sealed class DepthFeatureEngine
    {
        public DepthSecondFeature Calculate(
            DepthSnapshot snapshot)
        {
            if (snapshot.Bids.Count < 10 ||
                snapshot.Asks.Count < 10)
            {
                return new DepthSecondFeature
                {
                    TimestampUtc = DateTime.UtcNow
                };
            }

            var top5Bid =
                snapshot.Bids
                    .Take(5)
                    .Sum(x => x.Quantity);

            var top5Ask =
                snapshot.Asks
                    .Take(5)
                    .Sum(x => x.Quantity);

            var top10Bid =
                snapshot.Bids
                    .Take(10)
                    .Sum(x => x.Quantity);

            var top10Ask =
                snapshot.Asks
                    .Take(10)
                    .Sum(x => x.Quantity);

            var imbalanceTop5 =
                CalculateImbalance(
                    top5Bid,
                    top5Ask);

            var imbalanceTop10 =
                CalculateImbalance(
                    top10Bid,
                    top10Ask);

            var avgTop5Bid =
                snapshot.Bids
                    .Take(5)
                    .Average(x => (double)x.Quantity);

            var avgTop5Ask =
                snapshot.Asks
                    .Take(5)
                    .Average(x => (double)x.Quantity);

            var largestBid =
                (double)snapshot.Bids
                    .Take(5)
                    .Max(x => x.Quantity);

            var largestAsk =
                (double)snapshot.Asks
                    .Take(5)
                    .Max(x => x.Quantity);

            return new DepthSecondFeature
            {
                TimestampUtc = snapshot.TimestampUtc,

                Price =
        snapshot.Bids.Count > 0
            ? snapshot.Bids[0].Price
            : 0,

                ImbalanceTop5 = imbalanceTop5,

                ImbalanceTop10 = imbalanceTop10,

                LargestBidStrength =
        avgTop5Bid <= 0
            ? 0
            : largestBid / avgTop5Bid,

                LargestAskStrength =
        avgTop5Ask <= 0
            ? 0
            : largestAsk / avgTop5Ask
            };
        }

        private static double CalculateImbalance(
            decimal bidQty,
            decimal askQty)
        {
            var total =
                bidQty + askQty;

            if (total <= 0)
                return 0;

            return (double)
                ((bidQty - askQty) / total);
        }
    }
}
