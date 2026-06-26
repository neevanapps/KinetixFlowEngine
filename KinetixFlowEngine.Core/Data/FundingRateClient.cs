using System.Globalization;
using System.Text.Json;
using KinetixFlowEngine.Core.Domain.FundingRate;

namespace KinetixFlowEngine.Core.Data;

public sealed class FundingRateClient
{
    private readonly HttpClient _http;

    public FundingRateClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.bybit.com");
    }

    public async Task<FundingObservation?> GetCurrentAsync(
        string symbol = "BTCUSDT")
    {
        try
        {
            var response =
                await _http.GetAsync(
                    $"/v5/market/tickers?category=linear&symbol={symbol}");

            if (!response.IsSuccessStatusCode)
                return null;

            using var stream =
                await response.Content.ReadAsStreamAsync();

            using var doc =
                await JsonDocument.ParseAsync(stream);

            var list =
                doc.RootElement
                    .GetProperty("result")
                    .GetProperty("list");

            if (list.GetArrayLength() == 0)
                return null;

            var ticker = list[0];

            var rate =
                decimal.Parse(
                    ticker.GetProperty("fundingRate").GetString()!,
                    CultureInfo.InvariantCulture);

            return new FundingObservation
            {
                TimestampUtc = DateTime.UtcNow,

                Rate = rate
            };
        }
        catch
        {
            return null;
        }
    }
}