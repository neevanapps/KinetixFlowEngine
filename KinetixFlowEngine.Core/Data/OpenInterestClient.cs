using System.Globalization;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Data
{
    public class OpenInterestClient
    {
        private readonly HttpClient _http;

        public OpenInterestClient(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://fapi.binance.com");
        }

        public async Task<double> GetOpenInterestAsync(string symbol = "BTCUSDT")
        {
            var resp = await _http.GetAsync($"/fapi/v1/openInterest?symbol={symbol}");
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            // Response:
            // { "openInterest": "123456.789", "symbol": "BTCUSDT" }

            var oiStr = doc.RootElement
                .GetProperty("openInterest")
                .GetString()!;

            return double.Parse(oiStr, CultureInfo.InvariantCulture);
        }
    }
}
