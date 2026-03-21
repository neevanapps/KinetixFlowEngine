using KinetixFlowEngine.Core.Execution;
using KinetixFlowEngine.Core.Strategy;
using System;

namespace KinetixFlowEngine.Core.Trading
{
    public interface ITradeExecutor
    {
        Task<ExecutionResult> ExecuteAsync(ExecutionRequest request);
        Task<bool> ReducePositionAsync(ExecutionRequest executionRequest, decimal reduceQty);
        Task UpdateStopLossAsync(ExecutionRequest executionRequest, decimal stopLoss);
        Task ClosePositionAsync(ExecutionRequest request);
    }

    public class SimulatedExecutor : ITradeExecutor
    {
        private readonly Random _rand = new();

        public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request)
        {
            // simulate latency
            await Task.Delay(_rand.Next(50, 150));

            // simulate slippage (BTC realistic: $1–$5)
            var slip = (decimal)(_rand.NextDouble() * 4.0 - 2.0); // -2 to +2

            var filledPrice = request.Direction == SignalDirection.Long
                ? request.Price + slip
                : request.Price - slip;

            return new ExecutionResult
            {
                Success = true,
                FilledPrice = filledPrice,
                FilledQuantity = request.Quantity,
                OrderId = Guid.NewGuid().ToString()
            };
        }
        // -----------------------------
        // PARTIAL CLOSE (SIMULATION)
        // -----------------------------
        public async Task<bool> ReducePositionAsync(ExecutionRequest request, decimal reduceQty)
        {
            await Task.Delay(_rand.Next(20, 80));

            // No real exchange → just assume success
            return true;
        }

        // -----------------------------
        // STOP LOSS UPDATE (SIMULATION)
        // -----------------------------
        public async Task UpdateStopLossAsync(ExecutionRequest request, decimal stopLoss)
        {
            await Task.Delay(_rand.Next(20, 80));

            // No-op in simulation
        }

        public async Task ClosePositionAsync(ExecutionRequest request)
        {
            await Task.Delay(_rand.Next(20, 80));

            // No-op in simulation
        }
    }
}