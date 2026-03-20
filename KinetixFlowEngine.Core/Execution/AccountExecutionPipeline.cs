using CryptoExchange.Net.Requests;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Prop;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Execution
{
    public interface IAccountExecutionPipeline
    {
        void Execute(string accountId, StrategySignal signal, decimal price, decimal atr, KinetixEngineResult context);
    }

    public class AccountExecutionPipeline : IAccountExecutionPipeline
    {
        private readonly List<AccountRuntime> _accounts;
        private readonly ITradeExecutor _executor;
        private readonly PositionManager _positionManager;
        private readonly IPositionSizer _positionSizer;

        public AccountExecutionPipeline(
            List<AccountRuntime> accounts,
            ITradeExecutor executor,
            PositionManager positionManager,
            IPositionSizer positionSizer)
        {
            _accounts = accounts;
            _executor = executor;
            _positionManager = positionManager;
            _positionSizer = positionSizer;
        }

        public void Execute(string accountId, StrategySignal signal, decimal price, decimal atr, KinetixEngineResult context)
        {
            var acc = _accounts.First(x => x.Config.AccountId == accountId);

            // -------------------------------
            // GUARD
            // -------------------------------
            var guard = acc.Guard.EvaluateEntry(acc.Config, acc.State);
            if (!guard.Allowed)
                return;

            if (!acc.Config.Enabled || acc.State.IsPaused || acc.State.IsStopped)
                return;

            if (_positionManager.HasPosition(signal.StrategyName, accountId))
                return;

            // -------------------------------
            // STOP LOSS
            // -------------------------------
            var stopDistance = Math.Min(price * 0.01m, atr * Convert.ToDecimal(2.5));
            var stopLoss = signal.Direction == SignalDirection.Long
                ? price - stopDistance
                : price + stopDistance;

            // -------------------------------
            // SIZE
            // -------------------------------
            var size = _positionSizer.CalculateSize(acc, price, stopLoss);

            if (size <= 0)
                return;

            // -------------------------------
            // BUILD REQUEST
            // -------------------------------
            var request = new ExecutionRequest
            {
                AccountId = acc.Config.AccountId,
                ApiKey = acc.Config.ApiKey,
                ApiSecret = acc.Config.ApiSecret,
                StrategyName = signal.StrategyName,
                Direction = signal.Direction,
                Price = price,
                Quantity = size,
                StopLoss = stopLoss,
                TakeProfit = signal.Direction == SignalDirection.Long
                    ? price + (price - stopLoss)
                    : price - (stopLoss - price)
            };

            // -------------------------------
            // EXECUTE ORDER (CRITICAL)
            // -------------------------------
            var result = _executor.ExecuteAsync(request).Result;

            if (!result.Success)
                return;

            // -------------------------------
            // USE FILLED VALUES
            // -------------------------------
            _positionManager.TryEnterTrade(
                signal,
                result.FilledPrice,
                (double)atr,
                context,
                result.FilledQuantity,
                accountId,
                result.OrderId);
        }
    }
}