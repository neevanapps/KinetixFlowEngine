using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace KinetixFlowEngine.DataBuilder
{
    public class BinanceTradeDownloader
    {
        private readonly HttpClient _http;

        public BinanceTradeDownloader()
        {
            _http = new HttpClient();
            _http.BaseAddress = new Uri("https://fapi.binance.com");
        }

        public async Task DownloadAsync(
            string symbol,
            long startTime,
            long endTime,
            TradeBinaryWriter writer)
        {
            long current = startTime;

            while (current < endTime)
            {
                var url = $"/fapi/v1/aggTrades?symbol={symbol}&startTime={current}&endTime={endTime}&limit=1000";

                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var trades = JsonSerializer.Deserialize<List<BinanceAggTrade>>(json);

                if (trades == null || trades.Count == 0)
                    break;

                foreach (var t in trades)
                {
                    var trade = new ReplayTrade
                    {
                        Timestamp = t.T,
                        Price = double.Parse(t.p, CultureInfo.InvariantCulture),
                        Quantity = double.Parse(t.q, CultureInfo.InvariantCulture),
                        IsBuyerMaker = t.m
                    };

                    writer.Write(trade);

                    current = t.T + 1;
                }
            }
        }
    }
}
