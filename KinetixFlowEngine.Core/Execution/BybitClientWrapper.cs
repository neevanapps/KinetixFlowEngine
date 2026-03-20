using Bybit.Net;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;

namespace KinetixFlowEngine.Core.Execution
{
    public class BybitClientWrapper
    {
        private readonly BybitRestClient _client;

        public BybitClientWrapper(string apiKey, string secret)
        {
            _client = new BybitRestClient(options =>
            {
                options.ApiCredentials = new ApiCredentials(apiKey, secret);

                options.Environment = BybitEnvironment.DemoTrading;
                
                options.RequestTimeout = TimeSpan.FromSeconds(15);
            });
        }

        public BybitRestClient Client => _client; // ✅ ADD THIS
        public async Task<List<ExchangePosition>> GetOpenPositionsAsync(string accountId)
        {
            var result = await _client.V5Api.Trading.GetPositionsAsync(
                category: Category.Linear,
                symbol: "BTCUSDT");

            var list = new List<ExchangePosition>();

            if (!result.Success)
                return list;

            foreach (var p in result.Data.List)
            {
                if (p.Quantity == 0)
                    continue;

                list.Add(new ExchangePosition
                {
                    OrderId = $"{accountId}_{p.PositionIdx}", // better fallback
                    EntryPrice = p.AveragePrice ?? 0,
                    Quantity = p.Quantity,
                    AccountId = accountId // ✅ FIXED
                });
            }

            return list;
        }

        public async Task<ExchangePosition?> GetPositionAsync(string accountId)
        {
            var result = await _client.V5Api.Trading.GetPositionsAsync(
                category: Category.Linear,
                symbol: "BTCUSDT");

            if (!result.Success)
                return null;

            var pos = result.Data.List.FirstOrDefault(x => x.Quantity > 0);

            if (pos == null)
                return null;

            return new ExchangePosition
            {
                AccountId = accountId,
                EntryPrice = pos.AveragePrice ?? 0,
                Quantity = pos.Quantity
            };
        }
    }
}