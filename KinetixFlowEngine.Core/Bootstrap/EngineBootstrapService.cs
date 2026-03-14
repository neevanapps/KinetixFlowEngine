using Binance.Net.Clients;
using Binance.Net.Enums;
using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;
using Serilog;
using System.Runtime.InteropServices;

namespace KinetixFlowEngine.Core.Bootstrap
{
    public class EngineBootstrapService
    {
        private readonly BinanceRestClient _binance;
        private readonly AtrEngine _atr1m;
        private readonly AtrEngine _atr15m;
        private readonly EfficiencyRatioEngine _er;
        private readonly EfficiencyRatio30mEngine _er30;
        private readonly PriceTrendEngine _priceTrend;
        private readonly VwapEngine _vwap;
        private readonly ILogger<EngineBootstrapService> _logger;

        public EngineBootstrapService(EfficiencyRatioEngine er, PriceTrendEngine priceTrend, VwapEngine vwap, ILogger<EngineBootstrapService> logger, AtrEngine atr1m, Atr15mEngine atr15m, EfficiencyRatio30mEngine er30)
        {
            _binance = new BinanceRestClient();
            _atr1m = atr1m;
            _atr15m = atr15m;
            _er = er;
            _priceTrend = priceTrend;
            _vwap = vwap;
            _logger = logger;
            _er30 = er30;
        }

        public async Task InitializeAsync(bool isReplay)
        {
            _atr15m.Reset();
            _atr1m.Reset();
            if (isReplay)
            {
                _logger.LogInformation("REPLAY MODE: Skipping Binance Bootstrap. Engine will warm up from data file.");
                return;
            }
            _logger.LogInformation("BOOTSTRAP | Loading historical candles");

            var klines = await _binance.UsdFuturesApi.ExchangeData.GetKlinesAsync(
                "BTCUSDT",
                KlineInterval.OneMinute,
                limit: 720);

            if (!klines.Success)
                throw new Exception("Failed to fetch bootstrap klines");

            var candles = klines.Data;
            if (candles.Count() < 100)
                throw new Exception("Not enough bootstrap candles");
            // ---------- ATR 1m ----------
            foreach (var c in candles)
            {
                _atr1m.Update(
                    (double)c.HighPrice,
                    (double)c.LowPrice,
                    (double)c.ClosePrice);
            }

            // ---------- Build 15m candles ----------
            for (int i = 0; i < candles.Count(); i += 15)
            {
                var group = candles.Skip(i).Take(15).ToList();
                if (group.Count < 15) break;

                double high = group.Max(x => (double)x.HighPrice);
                double low = group.Min(x => (double)x.LowPrice);
                double close = (double)group.Last().ClosePrice;

                _atr15m.Update(high, low, close);
            }

            // ---------- ER + EMA bootstrap ----------
            var erCandles = candles.TakeLast(60);

            foreach (var c in erCandles)
            {
                var close = (double)c.ClosePrice;

                var er5 = _er.Update(close);
                var er30 = _er30.Update(close);

                _priceTrend.Update((decimal)close, (decimal)er5);
            }

            // ---------- VWAP bootstrap (15 minutes) ----------
            var vwapCandles = candles.TakeLast(15);

            foreach (var c in vwapCandles)
            {
                _vwap.Update(
                    c.ClosePrice,
                    c.Volume,
                    new DateTimeOffset(c.CloseTime).ToUnixTimeMilliseconds());
            }

            _logger.LogInformation(
                "BOOTSTRAP COMPLETE | ATR1m {ATR1:F2} ATR15m {ATR15:F2}",
                _atr1m.Value,
                _atr15m.Value);
        }
    }
}