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
        public ExecutionSyncService(PositionManager positions, ILogger<ExecutionSyncService> logger, BybitClientFactory factory, PropAccountRuntimeManager accounts, TelegramService telegramService)
        {
            _positions = positions;
            _logger = logger;
            _factory = factory;
            _accounts = accounts;
            _telegramService = telegramService;
        }

        public async Task SyncAsync()
        {
            try
            {
                foreach (var acc in _accounts.Accounts)
                {
                    var accountId = acc.Config.AccountId;

                    var client = _factory.GetClient(
                        accountId,
                        acc.Config.ApiKey,
                        acc.Config.ApiSecret);

                    var positions = await client.GetOpenPositionsAsync(accountId);

                    var prevPositions = _lastSnapshot.ContainsKey(accountId)
                        ? _lastSnapshot[accountId]
                        : new List<ExchangePosition>();

                    // -------------------------
                    // 1. DETECT CLOSED POSITIONS
                    // -------------------------
                    foreach (var prev in prevPositions)
                    {
                        var stillExists = positions.Any(x =>
                            Math.Abs(x.EntryPrice - prev.EntryPrice) < 2 &&
                            Math.Abs(x.Quantity - prev.Quantity) < 0.0005m);

                        if (!stillExists)
                        {
                            // CLOSED ON EXCHANGE
                            var local = _positions.GetAllPositions().FirstOrDefault(x =>
                                x.AccountId == accountId &&
                                Math.Abs(x.EntryPrice - prev.EntryPrice) < 2 &&
                                Math.Abs(x.InitialSize - prev.Quantity) < 0.0005m);

                            if (local != null && !local.Closed)
                            {
                                _logger.LogWarning("EXIT SYNC: Closing trade from exchange {OrderId}", local.OrderId);

                                _positions.CloseTrade(
                                    local.StrategyName,
                                    local.AccountId,
                                    prev.EntryPrice, // fallback exit price
                                    "ExchangeExit");
                            }
                        }
                    }

                    // -------------------------
                    // 2. ADD MISSING TRADES (existing logic)
                    // -------------------------
                    foreach (var ex in positions)
                    {
                        var local = _positions.GetAllPositions().FirstOrDefault(x =>
                            x.AccountId == ex.AccountId &&
                            Math.Abs(x.EntryPrice - ex.EntryPrice) < 2 &&
                            Math.Abs(x.InitialSize - ex.Quantity) < 0.0005m);

                        if (local == null)
                        {
                            _logger.LogWarning("Recovered missing trade {OrderId}", ex.OrderId);
                            // Await the Task returned by SendMessageAsync instead of calling .Wait()
                            await _telegramService.SendMessageAsync($"Strange: Recovered missing trade {ex.OrderId} on account {accountId}");
                            _positions.RestoreFromExchange(ex);
                        }
                    }

                    // -------------------------
                    // 3. UPDATE SNAPSHOT
                    // -------------------------
                    _lastSnapshot[accountId] = positions;

                    _logger.LogInformation(
                        "Sync: Account={acc} Exchange={count} Local={localCount}",
                        accountId,
                        positions.Count,
                        _positions.GetAllPositions().Count(x => x.AccountId == accountId));
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
    }
}