using Bybit.Net.Enums;
using KinetixFlowEngine.Core.Execution;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Execution
{
    public class BybitExecutor : ITradeExecutor
    {
        private readonly BybitClientFactory _factory;

        public BybitExecutor(BybitClientFactory factory)
        {
            _factory = factory;
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
                decimal sanitizedQty = Math.Round(request.Quantity, 3, MidpointRounding.ToZero);
 
                var client = _factory.GetClient(
                    request.AccountId,
                    request.ApiKey,
                    request.ApiSecret);

                var side = request.Direction == SignalDirection.Long
                    ? OrderSide.Buy
                    : OrderSide.Sell;

                // -----------------------------
                // PLACE ORDER
                // -----------------------------
                var order = await client.Client.V5Api.Trading.PlaceOrderAsync(
                    category: Category.Linear,
                    symbol: "BTCUSDT",
                    side: side,
                    type: NewOrderType.Market,
                    quantity: sanitizedQty,
                    timeInForce: TimeInForce.GoodTillCanceled); // ✅ FIXED

                if (!order.Success)
                {
                    return new ExecutionResult
                    {
                        Success = false,
                        Error = order.Error?.Message ?? "Order failed"
                    };
                }

                var orderId = order.Data.OrderId;
                // -----------------------------
                // WAIT FOR POSITION CREATION
                // -----------------------------
                await Task.Delay(300); // wait for position update

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
                    FilledPrice = fillPrice,
                    FilledQuantity = fillQty
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

            var result = await client.Client.V5Api.Trading.PlaceOrderAsync(
                category: Category.Linear,
                symbol: "BTCUSDT",
                side: side,
                type: NewOrderType.Market,
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
    }
}