using System.Globalization;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Data
{
    public class FundingRateClient
    {
        private readonly HttpClient _http;

        public FundingRateClient(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://api.bybit.com");
        }

        public async Task<double> GetCurrentFundingRateAsync(string symbol = "BTCUSDT")
        {
            try
            {
                var resp = await _http.GetAsync($"/v5/market/tickers?category=linear&symbol={symbol}");

                if (!resp.IsSuccessStatusCode)
                    return 0.0;

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var list = doc.RootElement
                    .GetProperty("result")
                    .GetProperty("list");

                if (list.GetArrayLength() == 0)
                    return 0.0;

                var ticker = list[0];

                var fundingRateStr = ticker.GetProperty("fundingRate").GetString() ?? "0";

                return double.Parse(fundingRateStr, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0.0;
            }
        }
    }
}