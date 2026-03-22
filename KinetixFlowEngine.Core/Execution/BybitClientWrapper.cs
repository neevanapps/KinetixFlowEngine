using Bybit.Net;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using KinetixFlowEngine.Core.Prop;

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

        /// <summary>
        /// Returns actual USDT available balance from Bybit demo account
        /// </summary>
        public async Task<decimal> GetUsdtWalletBalanceAsync()
        {
            var result = await _client.V5Api.Account.GetBalancesAsync(
                accountType: AccountType.Unified,
                asset: "USDT");

            if (!result.Success || result.Data == null || !result.Data.List.Any())
            {
                return 0;
            }

            var account = result.Data.List.FirstOrDefault();
            var usdtAsset = account?.Assets.FirstOrDefault(a => a.Asset == "USDT");

            return usdtAsset?.WalletBalance ?? 0m;
        }

        public async Task<decimal> GetUsdtEquityBalanceAsync()
        {
            var result = await _client.V5Api.Account.GetBalancesAsync(
                accountType: AccountType.Unified,
                asset: "USDT");

            if (!result.Success || result.Data == null || !result.Data.List.Any())
            {
                return 0;
            }

            var account = result.Data.List.FirstOrDefault();
            var usdtAsset = account?.Assets.FirstOrDefault(a => a.Asset == "USDT");

            return usdtAsset?.Equity ?? 0m;
        }

        /// <summary>
        /// Returns current maker/taker fee rate for BTCUSDT Linear (as decimal, e.g. 0.0001 = 0.01%)
        /// </summary>
        public async Task<(decimal MakerFee, decimal TakerFee)> GetFeeRatesAsync()
        {
            var result = await _client.V5Api.Account.GetFeeRateAsync(
                category: Category.Linear,
                symbol: "BTCUSDT");

            if (!result.Success || result.Data == null || !result.Data.List.Any())
            {
                // Fallback to standard Bybit rates
                return (0.0001m, 0.00055m);
            }

            var fee = result.Data.List[0];
            return (fee.MakerFeeRate, fee.TakerFeeRate);
        }

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
                    AccountId = accountId,
                    PositionSide=p.Side
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
                Quantity = pos.Quantity,
                PositionSide = pos.Side
            };
        }
    }
}