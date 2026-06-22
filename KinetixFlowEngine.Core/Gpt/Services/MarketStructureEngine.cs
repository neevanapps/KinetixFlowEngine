using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Database;
using KinetixFlowEngine.Core.Gpt.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Gpt.Services
{
    public sealed class MarketStructureEngine
    {
        private readonly OhlcAggregator _ohlc = new();

        public MarketStructureSnapshot Build(
            IReadOnlyList<MarketPriceEntity> prices,
            decimal currentPrice,
            decimal vwap)
        {
            var p10 = prices.TakeLast(KinetixConstants.Level1).ToList();
            var p30 = prices.TakeLast(KinetixConstants.Level2).ToList();
            var p60 = prices.TakeLast(KinetixConstants.Level3).ToList();

            var c10 = _ohlc.Build(p10);
            var c30 = _ohlc.Build(p30);
            var c60 = _ohlc.Build(p60);

            return new MarketStructureSnapshot
            {
                Trend10m = GetTrend(c10),
                Trend30m = GetTrend(c30),
                Trend60m = GetTrend(c60),

                DistanceFrom10mHigh =
                    Distance(currentPrice, c10.High),

                DistanceFrom10mLow =
                    Distance(currentPrice, c10.Low),

                DistanceFrom30mHigh =
                    Distance(currentPrice, c30.High),

                DistanceFrom30mLow =
                    Distance(currentPrice, c30.Low),

                DistanceFrom60mHigh =
                    Distance(currentPrice, c60.High),

                DistanceFrom60mLow =
                    Distance(currentPrice, c60.Low),

                DistanceFromVWAP =
                    (double)(currentPrice - vwap),

                DistanceFromVWAPPct =
                    vwap == 0
                        ? 0
                        : (double)((currentPrice - vwap) / vwap),

                RangeHigh10m = c10.High,
                RangeLow10m = c10.Low,

                RangeHigh30m = c30.High,
                RangeLow30m = c30.Low,

                RangeHigh60m = c60.High,
                RangeLow60m = c60.Low,

                Candle10m = c10,
                Candle30m = c30,
                Candle60m = c60
            };
        }

        private static string GetTrend(
            CandleSnapshot candle)
        {
            if (candle.Close > candle.Open)
                return "Bullish";

            if (candle.Close < candle.Open)
                return "Bearish";

            return "Neutral";
        }

        private static double Distance(
            decimal current,
            decimal level)
        {
            if (current == 0)
                return 0;

            return (double)((current - level) / current);
        }
    }
}
