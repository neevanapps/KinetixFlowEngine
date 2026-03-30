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
                var resp = await _http.GetAsync($"/v5/market/funding/history?category=linear&symbol={symbol}&limit=1");

                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[FundingRate] HTTP {(int)resp.StatusCode} - {resp.ReasonPhrase}");
                    return 0.0;
                }

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var result = doc.RootElement.GetProperty("result");
                var list = result.GetProperty("list");

                if (list.GetArrayLength() == 0)
                    return 0.0;

                var fundingRateStr = list[0].GetProperty("fundingRate").GetString() ?? "0";
                return double.Parse(fundingRateStr, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FundingRateClient] Error: {ex.Message}");
                return 0.0;
            }
        }
    }
}