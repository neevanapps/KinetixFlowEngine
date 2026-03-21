using Bybit.Net.Enums;
using CryptoExchange.Net.Requests;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Execution;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;
using System.Drawing;

namespace KinetixFlowEngine.Core.Execution
{
    public class BybitExecutor : ITradeExecutor
    {
        private readonly BybitClientFactory _factory;
        private readonly BybitDepthStreamClient _depth;
        private readonly ILogger<BybitExecutor> _logger;
        private const decimal TICK = 0.5m;
        private const int MAX_RETRIES = 5;
        public BybitExecutor(BybitClientFactory factory, BybitDepthStreamClient client, ILogger<BybitExecutor> logger)
        {
            _factory = factory;
            _depth = client;
            _logger = logger;
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
            var client = _factory.GetClient(
                req.AccountId,
                req.ApiKey,
                req.ApiSecret);

            var side = req.Direction == SignalDirection.Long
                ? OrderSide.Sell
                : OrderSide.Buy;

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                var price = GetAdaptiveExitPrice(side, attempt);

                if (price == 0)
                {
                    _logger.LogWarning("No market data for exit");
                    break;
                }

                _logger.LogInformation("EXIT Attempt {Attempt} | Price {Price}", attempt, price);

                var order = await client.Client.V5Api.Trading.PlaceOrderAsync(
                    category: Category.Linear,
                    symbol: "BTCUSDT",
                    side: side,
                    type: NewOrderType.Limit,
                    price: price,
                    quantity: req.Quantity,
                    reduceOnly: true,
                    timeInForce: TimeInForce.PostOnly);

                if (order.Success)
                {
                    _logger.LogInformation("Exit filled as MAKER");
                    return;
                }

                _logger.LogWarning("Exit PostOnly rejected, retrying...");

                await Task.Delay(GetBackoff(attempt));
            }

            // 🔴 FINAL FALLBACK (MANDATORY)
            _logger.LogWarning("EXIT fallback → MARKET");

            await client.Client.V5Api.Trading.PlaceOrderAsync(
                category: Category.Linear,
                symbol: "BTCUSDT",
                side: side,
                type: NewOrderType.Market,
                quantity: req.Quantity,
                reduceOnly: true);
        }

        public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request)
        {
            try
            {
                if (request.Quantity <= 0)
                {
                    return new ExecutionResult
                    {
                        Success = false,
                        Error = "Invalid size"
                    };
                }

                var client = _factory.GetClient(
                    request.AccountId,
                    request.ApiKey,
                    request.ApiSecret);

                var side = request.Direction == SignalDirection.Long
                    ? OrderSide.Buy
                    : OrderSide.Sell;

                for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
                {
                    var price = GetAdaptivePrice(side, attempt);

                    if (price == 0)
                    {
                        return new ExecutionResult
                        {
                            Success = false,
                            Error = "No market data"
                        };
                    }

                    _logger.LogInformation("Attempt {Attempt} | Price {Price}", attempt, price);

                    var order = await client.Client.V5Api.Trading.PlaceOrderAsync(
                        category: Category.Linear,
                        symbol: "BTCUSDT",
                        side: side,
                        type: NewOrderType.Limit,
                        price: price,
                        quantity: request.Quantity,
                        reduceOnly: false,
                        timeInForce: TimeInForce.PostOnly);

                    if (order.Success)
                    {
                        var orderId = order.Data.OrderId;

                        await Task.Delay(300);

                        var pos = await client.GetPositionAsync(request.AccountId);
                        decimal fillPrice = pos?.EntryPrice ?? request.Price;
                        decimal fillQty = pos?.Quantity ?? request.Quantity;

                        // -----------------------------
                        // SET SL / TP
                        // -----------------------------
                        await client.Client.V5Api.Trading.SetTradingStopAsync(
                            category: Category.Linear,
                            symbol: "BTCUSDT",
                            stopLoss: request.StopLoss,
                            takeProfit: request.TakeProfit,
                            positionIdx: 0);
                        return new ExecutionResult
                        {
                            Success = true,
                            OrderId = orderId,
                            FilledPrice = pos?.EntryPrice ?? price,
                            FilledQuantity = pos?.Quantity ?? request.Quantity
                        };
                    }

                    _logger.LogWarning("PostOnly rejected. Retrying...");

                    await Task.Delay(GetBackoff(attempt));
                }

                return new ExecutionResult
                {
                    Success = false,
                    Error = "PostOnly retries exhausted"
                };
            }
            catch (Exception ex)
            {
                return new ExecutionResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
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
                    quantity: request.Quantity,
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
            return 150 * (int)Math.Pow(2, attempt);
        }
    }
}