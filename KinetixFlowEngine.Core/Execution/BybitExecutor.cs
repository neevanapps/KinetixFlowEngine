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
        private readonly TelegramService _telegram;
        private readonly BybitClientFactory _factory;
        private readonly BybitDepthStreamClient _depth;
        private readonly ILogger<BybitExecutor> _logger;
        private const decimal TICK = 0.5m;
        private const int MAX_RETRIES = 5;
        public BybitExecutor(BybitClientFactory factory, BybitDepthStreamClient client, ILogger<BybitExecutor> logger, TelegramService telegram)
        {
            _factory = factory;
            _depth = client;
            _logger = logger;
            _telegram = telegram;
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

            // Step 1: Get current position to determine real direction & size
            var pos = await client.GetPositionAsync(req.AccountId);
            if (pos == null || pos.Quantity <= 0)
            {
                _logger.LogInformation("ClosePositionAsync: No open position to close for {Account}", req.AccountId);
                return; // Nothing to do
            }

            // Real position direction &     size
            var realDirection = pos.Quantity > 0 ? SignalDirection.Long : SignalDirection.Short;
            var closeSide = pos.PositionSide == PositionSide.Buy ? OrderSide.Sell : OrderSide.Buy;
            var closeQty = pos.Quantity; // use actual position size (handles partials)

            _logger.LogInformation("Closing position: {Direction} | Size: {Qty} | Side: {CloseSide}",
                realDirection, closeQty, closeSide);

            string orderId = null;

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                var price = GetAdaptiveExitPrice(closeSide, attempt);
                if (price == 0)
                {
                    _logger.LogWarning("No market data for exit");
                    break;
                }

                _logger.LogInformation("EXIT Attempt {Attempt} | Price {Price} | Side {Side}", attempt, price, closeSide);

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
                    _logger.LogWarning("Exit place failed: {Error}", orderResponse.Error?.Message);
                    await Task.Delay(GetBackoff(attempt));
                    continue;
                }

                orderId = orderResponse.Data.OrderId;
                _logger.LogInformation("Exit order placed: {OrderId}", orderId);

                // Wait for fill
                bool filled = await WaitForOrderFill(client, orderId, closeQty, TimeSpan.FromSeconds(30));

                if (filled)
                {
                    _logger.LogInformation("Exit filled as MAKER (order {OrderId})", orderId);
                    return; // Success
                }

                // Cancel and retry
                await client.Client.V5Api.Trading.CancelOrderAsync(
                    category: Category.Linear,
                    symbol: "BTCUSDT",
                    orderId: orderId);

                _logger.LogWarning("Exit order {OrderId} not filled → cancelled, retrying", orderId);
                await Task.Delay(GetBackoff(attempt));
            }

            // Final fallback: market close
            _logger.LogWarning("All PostOnly exit retries failed → falling back to MARKET close");

            var marketClose = await client.Client.V5Api.Trading.PlaceOrderAsync(
                category: Category.Linear,
                symbol: "BTCUSDT",
                side: closeSide,
                type: NewOrderType.Market,
                quantity: closeQty,
                reduceOnly: true);

            if (marketClose.Success)
            {
                _logger.LogInformation("Market close executed successfully");
            }
            else
            {
                _logger.LogError("Market close FAILED: {Error}", marketClose.Error?.Message);
                await _telegram.SendMessageAsync($"CRITICAL: Market close FAILED for {req.AccountId}: {marketClose.Error?.Message}");
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