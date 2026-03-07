using Binance.Net.Clients;
using Binance.Net.Enums;
using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Bootstrap
{
    public class EngineBootstrapService
    {
        private readonly BinanceRestClient _binance;
        private readonly AtrEngine _atr1m;
        private readonly AtrEngine _atr15m;
        private readonly EfficiencyRatioEngine _er;
        private readonly PriceTrendEngine _priceTrend;
        private readonly VwapEngine _vwap;
        private readonly ILogger<EngineBootstrapService> _logger;

        public EngineBootstrapService(EfficiencyRatioEngine er, PriceTrendEngine priceTrend, VwapEngine vwap, ILogger<EngineBootstrapService> logger)
        {
            _binance = new BinanceRestClient();
            _atr1m = new AtrEngine();
            _atr15m = new AtrEngine();
            _er = er;
            _priceTrend = priceTrend;
            _vwap = vwap;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            _atr15m.Reset();
            _atr1m.Reset();

            _logger.LogInformation("BOOTSTRAP | Loading historical candles");

            var klines = await _binance.UsdFuturesApi.ExchangeData.GetKlinesAsync(
                "BTCUSDT",
                KlineInterval.OneMinute,
                limit: 720);

            if (!klines.Success)
                throw new Exception("Failed to fetch bootstrap klines");

            var candles = klines.Data.ToList();

            // ---------- ATR 1m ----------
            foreach (var c in candles)
            {
                var high = (double)c.HighPrice;
                var low = (double)c.LowPrice;
                var close = (double)c.ClosePrice;

                _atr1m.Update(high, low, close);
            }

            // ---------- Build 15m candles ----------
            for (int i = 0; i < candles.Count; i += 15)
            {
                var group = candles.Skip(i).Take(15).ToList();
                if (group.Count < 15) break;

                double high = group.Max(x => (double)x.HighPrice);
                double low = group.Min(x => (double)x.LowPrice);
                double close = (double)group.Last().ClosePrice;

                _atr15m.Update(high, low, close);
            }

            // ---------- ER + EMA + VWAP ----------
            foreach (var c in candles)
            {
                double price = (double)c.ClosePrice;
                decimal volume = c.Volume;

                _er.Update(price);

                _priceTrend.Update((decimal)price, 0.5m); // neutral ER seed

                _vwap.Update((decimal)price, volume);
            }

            _logger.LogInformation(
                "BOOTSTRAP COMPLETE | ATR1m {ATR1:F2} ATR15m {ATR15:F2}",
                _atr1m.Value,
                _atr15m.Value);
        }
    }
}