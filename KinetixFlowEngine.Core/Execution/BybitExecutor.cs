using Bybit.Net.Enums;
using CryptoExchange.Net.Requests;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Execution;
using KinetixFlowEngine.Core.Prop;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Utils;
using System.Drawing;

namespace KinetixFlowEngine.Core.Execution
{
    public class BybitExecutor : ITradeExecutor
    {
        private readonly INotificationService _notificationService;
        private readonly BybitClientFactory _factory;
        private readonly BybitDepthStreamClient _depth;
        private readonly ILogger<BybitExecutor> _logger;
        private const decimal TICK = 0.5m;
        private const int MAX_RETRIES = 5;
        public BybitExecutor(BybitClientFactory factory, BybitDepthStreamClient client, ILogger<BybitExecutor> logger, INotificationService notificationService)
        {
            _factory = factory;
            _depth = client;
            _logger = logger;
            _notificationService = notificationService;
        }

        public decimal GetMakerPrice(OrderSide orderSide)
        {
            var best = _depth.Current;

            if (best == null || best.BestBid == 0)
                return 0;

            return orderSide == OrderSide.Buy
                ? best.BestBid - TICK     // BUY → below bid
                : best.BestAsk + TICK;    // SELL → above ask
        }

        public async Task ClosePositionAsync(ExecutionRequest req)
        {
            var client = _factory.GetClient(req.AccountId, req.ApiKey, req.ApiSecret);

            var pos = await client.GetPositionAsync(req.AccountId);
            if (pos == null || pos.Quantity <= 0)
            {
                _logger.LogInformation("ClosePositionAsync: No position to close");
                return;
            }

            var closeSide = pos.PositionSide == PositionSide.Buy ? OrderSide.Sell : OrderSide.Buy;
            var closeQty = pos.Quantity;   // real exchange qty

            _logger.LogInformation("FIRE-AND-FORGET CLOSE | {Direction} | Qty {Qty} | Account {Acc}",
                pos.PositionSide, closeQty, req.AccountId);

            // Place order and forget immediately
            var price = GetAdaptiveExitPrice(closeSide, 1);
            var orderResponse = await client.Client.V5Api.Trading.PlaceOrderAsync(
                category: Category.Linear,
                symbol: "BTCUSDT",
                side: closeSide,
                type: NewOrderType.Limit,
                price: price,
                quantity: closeQty,
                reduceOnly: true,
                timeInForce: TimeInForce.PostOnly);

            if (!orderResponse.Success)
            {
                _logger.LogWarning("Close order failed immediately: {Error}", orderResponse.Error?.Message);
                // fallback to market in background only
                _ = Task.Run(() => MarketCloseFallback(client, closeSide, closeQty, req.AccountId));
                return;
            }

            _logger.LogInformation("Close order placed (fire-and-forget) OrderId={Id}", orderResponse.Data.OrderId);

            // Optional: still try one quick market fallback after 15s if needed
            _ = Task.Delay(15000).ContinueWith(_ => MarketCloseFallback(client, closeSide, closeQty, req.AccountId));
        }

        private async Task MarketCloseFallback(BybitClientWrapper client, OrderSide side, decimal qty, string accountId)
        {
            try
            {
                await client.Client.V5Api.Trading.PlaceOrderAsync(
                    category: Category.Linear,
                    symbol: "BTCUSDT",
                    side: side,
                    type: NewOrderType.Market,
                    quantity: qty,
                    reduceOnly: true);
                _logger.LogInformation("Market fallback executed for remnant cleanup");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Market fallback failed");
                await _notificationService.SendMessageAsync($"CRITICAL remnant close failed {accountId}");
            }
        }

        public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request)
        {
            try
            {
                if (request.Quantity <= 0)
                {
                    return new ExecutionResult { Success = false, Error = "Invalid size" };
                }

                var client = _factory.GetClient(request.AccountId, request.ApiKey, request.ApiSecret);
                var side = request.Direction == SignalDirection.Long ? OrderSide.Buy : OrderSide.Sell;

                string orderId = null;
                decimal? actualFillPrice = null;
                decimal? actualFillQty = null;

                for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
                {
                    var price = GetAdaptivePrice(side, attempt);
                    if (price == 0)
                    {
                        return new ExecutionResult { Success = false, Error = "No market data" };
                    }

                    _logger.LogInformation("Attempt {Attempt} | Price {Price}", attempt, price);

                    var orderResponse = await client.Client.V5Api.Trading.PlaceOrderAsync(
                        category: Category.Linear,
                        symbol: "BTCUSDT",
                        side: side,
                        type: NewOrderType.Limit,
                        price: price,
                        quantity: request.Quantity,
                        reduceOnly: false,
                        timeInForce: TimeInForce.PostOnly);

                    if (!orderResponse.Success)
                    {
                        _logger.LogWarning("PlaceOrder failed: {Error}", orderResponse.Error?.Message);
                        await Task.Delay(GetBackoff(attempt));
                        continue;
                    }

                    orderId = orderResponse.Data.OrderId;
                    _logger.LogInformation("Order placed: {OrderId}", orderId);

                    // ── WAIT & POLL FOR FILL ────────────────────────────────────────
                    bool filled = await WaitForOrderFill(client, orderId, request.Quantity, TimeSpan.FromSeconds(30));

                    if (!filled)
                    {
                        // Optional: cancel unfilled order to clean up
                        await client.Client.V5Api.Trading.CancelOrderAsync(
                            category: Category.Linear,
                            symbol: "BTCUSDT",
                            orderId: orderId);

                        _logger.LogWarning("Order {OrderId} not filled in time → cancelled", orderId);
                        await Task.Delay(GetBackoff(attempt));
                        continue;
                    }

                    // ── Order is filled → get real position data ────────────────────
                    await Task.Delay(500); // small buffer for position update

                    var pos = await client.GetPositionAsync(request.AccountId);
                    if (pos == null || pos.Quantity <= 0)
                    {
                        _logger.LogError("Position not found after fill confirmation");
                        return new ExecutionResult { Success = false, Error = "Position missing after fill" };
                    }

                    actualFillPrice = pos.EntryPrice;
                    actualFillQty = pos.Quantity;

                    // ── NOW safe to attach SL ────────────────────────────────────
                    var slTpResult = await client.Client.V5Api.Trading.SetTradingStopAsync(
                        category: Category.Linear,
                        symbol: "BTCUSDT",
                        stopLoss: request.StopLoss,
                        positionIdx: 0);  // 0 = one-way mode; adjust if hedge mode

                    if (!slTpResult.Success)
                    {
                        _logger.LogError("Failed to set SL/TP: {Error}", slTpResult.Error?.Message);
                        // Decide: return failure or continue (position is open, SL/TP missing)
                        // For safety → perhaps close position if SL/TP critical
                    }
                    else
                    {
                        _logger.LogInformation("SL/TP attached successfully");
                    }

                    return new ExecutionResult
                    {
                        Success = true,
                        OrderId = orderId,
                        FilledPrice = actualFillPrice.Value,
                        FilledQuantity = actualFillQty.Value
                    };
                }

                return new ExecutionResult { Success = false, Error = "PostOnly retries exhausted" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteAsync exception");
                return new ExecutionResult { Success = false, Error = ex.Message };
            }
        }

        private async Task<bool> WaitForOrderFill(BybitClientWrapper client, string orderId, decimal originalQty, TimeSpan timeout)
        {
            var start = DateTime.UtcNow;

            while (DateTime.UtcNow - start < timeout)
            {
                var orderStatus = await client.Client.V5Api.Trading.GetOpenSpreadOrdersAsync(
                    symbol: "BTCUSDT",
                    orderId: orderId);

                if (orderStatus.Success && orderStatus.Data.List.Length > 0)
                {
                    var order = orderStatus.Data.List[0];
                    if (order.OrderStatus == OrderStatus.Filled || order.OrderStatus == OrderStatus.PartiallyFilled)
                    {
                        _logger.LogInformation("Order {OrderId} filled/partially filled. CumExecQty: {Qty}", orderId, order.QuantityFilled);
                        return true;
                    }
                }
                else
                {
                    // If not in open orders anymore → likely filled & moved to history
                    var history = await client.Client.V5Api.Trading.GetOrderHistoryAsync(
                        category: Category.Linear,
                        symbol: "BTCUSDT",
                        orderId: orderId);

                    if (history.Success && history.Data.List.Length > 0)
                    {
                        var histOrder = history.Data.List[0];
                        if (histOrder.Status == OrderStatus.Filled)
                        {
                            _logger.LogInformation("Order {OrderId} confirmed filled from history");
                            return true;
                        }
                    }
                }

                await Task.Delay(1000); // poll every 1s
            }

            _logger.LogWarning("Timeout waiting for fill on order {OrderId}", orderId);
            return false;
        }

        public async Task<bool> ReducePositionAsync(ExecutionRequest request, decimal reduceQty)
        {
            var client = _factory.GetClient(
                request.AccountId,
                request.ApiKey,
                request.ApiSecret);

            var side = request.Direction == SignalDirection.Long
                ? OrderSide.Sell
                : OrderSide.Buy;
            var price = GetMakerPrice(side);

            var result = await client.Client.V5Api.Trading.PlaceOrderAsync(
                    category: Category.Linear,
                    symbol: "BTCUSDT",
                    side: side,
                    type: NewOrderType.Limit,
                    price: price,
                    quantity: reduceQty,
                    reduceOnly: true);

            return result.Success;
        }

        private async Task<bool> WaitForFullClose(BybitClientWrapper client, string accountId, TimeSpan timeout)
        {
            var start = DateTime.UtcNow;

            while (DateTime.UtcNow - start < timeout)
            {
                var pos = await client.GetPositionAsync(accountId);

                if (pos == null || Math.Abs(pos.Quantity) < 0.00001m)
                {
                    _logger.LogInformation("Position fully closed");
                    return true;
                }

                _logger.LogInformation("Waiting for full close. Remaining qty: {Qty}", pos.Quantity);

                await Task.Delay(1000);
            }

            _logger.LogWarning("Timeout waiting for full close");
            return false;
        }

        public async Task UpdateStopLossAsync(ExecutionRequest request, decimal newStopLoss)
        {
            var client = _factory.GetClient(
                request.AccountId,
                request.ApiKey,
                request.ApiSecret);

            await client.Client.V5Api.Trading.SetTradingStopAsync(
                category: Category.Linear,
                symbol: "BTCUSDT",
                stopLoss: newStopLoss,
                positionIdx: 0);
        }

        private decimal GetAdaptivePrice(OrderSide side, int attempt)
        {
            var best = _depth.Current;

            if (best == null || best.BestBid == 0)
                return 0;

            var offset = TICK * attempt;

            return side == OrderSide.Buy
                ? best.BestBid - offset
                : best.BestAsk + offset;
        }

        private decimal GetAdaptiveExitPrice(OrderSide side, int attempt)
        {
            var best = _depth.Current;

            if (best == null || best.BestBid == 0)
                return 0;

            var offset = TICK * attempt;

            // Opposite of entry logic
            return side == OrderSide.Sell
                ? best.BestAsk + offset   // closing LONG → sell above ask
                : best.BestBid - offset;  // closing SHORT → buy below bid
        }

        private int GetBackoff(int attempt)
        {
            return 750 * (int)Math.Pow(2, attempt);
        }
    }
}