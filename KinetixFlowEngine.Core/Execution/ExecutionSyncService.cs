using Bybit.Net.Enums;
using KinetixFlowEngine.Core.Prop;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Execution
{
    public class ExecutionSyncService
    {
        private readonly PositionManager _positions;
        private readonly ILogger<ExecutionSyncService> _logger;
        private readonly BybitClientFactory _factory;
        private readonly PropAccountRuntimeManager _accounts;
        private readonly Dictionary<string, List<ExchangePosition>> _lastSnapshot = new();
        private readonly TelegramService _telegramService;
        private readonly PropAccountStatePersistence _accountStatePersistence;

        public ExecutionSyncService(PositionManager positions, ILogger<ExecutionSyncService> logger, BybitClientFactory factory,
            PropAccountRuntimeManager accounts, TelegramService telegramService, PropAccountStatePersistence accountStatePersistence)
        {
            _positions = positions;
            _logger = logger;
            _factory = factory;
            _accounts = accounts;
            _telegramService = telegramService;
            _accountStatePersistence = accountStatePersistence;
        }

        public async Task SyncAsync()
        {
            try
            {
                foreach (var acc in _accounts.Accounts)
                {
                    var accountId = acc.Config.AccountId;
                    var client = _factory.GetClient(accountId, acc.Config.ApiKey, acc.Config.ApiSecret);

                    // ==================== BALANCE SYNC (fixed) ====================
                    decimal realBalance = await client.GetUsdtWalletBalanceAsync();
                    if (realBalance > 0 && Math.Abs(realBalance - acc.State.CurrentEquity) > 0.01m)
                    {
                        decimal difference = realBalance - acc.State.CurrentEquity;
                        _logger.LogInformation("Balance sync | Account {Id} | Real={Real} → Internal={Internal} | Diff={Diff}",
                            accountId, realBalance, acc.State.CurrentEquity, difference);

                        acc.State.CurrentEquity = realBalance;
                        _accountStatePersistence.Update(accountId, acc.State);
                    }

                    var positions = await client.GetOpenPositionsAsync(accountId);

                    // ==================== TINY REMNANT AUTO-CLEANUP (fixed) ====================
                    foreach (var ex in positions)
                    {
                        if (ex.Quantity > 0 && ex.Quantity < 0.002m)
                        {
                            _logger.LogWarning("TINY REMNANT DETECTED {Acc} | Qty {Qty} → forcing market close",
                                accountId, ex.Quantity);

                            var side = ex.PositionSide == PositionSide.Buy ? OrderSide.Sell : OrderSide.Buy;

                            // Fire-and-forget market close (no waiting)
                            await client.Client.V5Api.Trading.PlaceOrderAsync(
                                category: Category.Linear,
                                symbol: "BTCUSDT",
                                side: side,
                                type: NewOrderType.Market,
                                quantity: ex.Quantity,
                                reduceOnly: true);

                            // Clean internal state using tolerant lookup (safer than OrderId)
                            _positions.ForceRemoveRemnantByPrice(accountId, ex.EntryPrice, ex.Quantity);
                        }
                    }

                    // Rest of your original logic (unchanged)
                    var prevPositions = _lastSnapshot.ContainsKey(accountId)
                        ? _lastSnapshot[accountId]
                        : new List<ExchangePosition>();

                    foreach (var prev in prevPositions)
                    {
                        var stillExists = positions.Any(x =>
                            Math.Abs(x.EntryPrice - prev.EntryPrice) < 2 &&
                            Math.Abs(x.Quantity - prev.Quantity) < 0.0005m);

                        if (!stillExists)
                        {
                            var local = _positions.GetAllPositions().FirstOrDefault(x =>
                                x.AccountId == accountId &&
                                Math.Abs(x.EntryPrice - prev.EntryPrice) < 2 &&
                                Math.Abs(x.InitialSize - prev.Quantity) < 0.0005m);

                            if (local != null && !local.Closed)
                            {
                                _logger.LogWarning("EXIT SYNC: Closing trade from exchange {OrderId}", local.OrderId);
                                _positions.CloseTrade(local.StrategyName, local.AccountId, prev.EntryPrice, "ExchangeExit");
                            }
                        }
                    }

                    foreach (var ex in positions)
                    {
                        var local = _positions.GetAllPositions().FirstOrDefault(x =>
                            x.AccountId == ex.AccountId &&
                            Math.Abs(x.EntryPrice - ex.EntryPrice) < 2 &&
                            Math.Abs(x.InitialSize - ex.Quantity) < 0.0005m);

                        if (local == null)
                        {
                            _logger.LogWarning("Recovered missing trade {OrderId}", ex.OrderId);
                            await _telegramService.SendMessageAsync($"Strange: Recovered missing trade {ex.OrderId} on account {accountId}");
                            _positions.RestoreFromExchange(ex);
                        }
                    }

                    _lastSnapshot[accountId] = positions;

                    _logger.LogInformation(
                        "Sync: Account={acc} Exchange={count} Local={localCount}",
                        accountId, positions.Count, _positions.GetAllPositions().Count(x => x.AccountId == accountId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution sync failed");
            }
        }
    }

    public class ExchangePosition
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal EntryPrice { get; set; }
        public decimal Quantity { get; set; }
        public string AccountId { get; set; } = string.Empty;
        public PositionSide? PositionSide { get; set; }
    }
}