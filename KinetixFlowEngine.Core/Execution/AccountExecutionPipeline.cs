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
        private readonly ILogger<AccountExecutionPipeline> _logger;

        public AccountExecutionPipeline(
            List<AccountRuntime> accounts,
            ITradeExecutor executor,
            PositionManager positionManager,
            IPositionSizer positionSizer,
            ILogger<AccountExecutionPipeline> logger)
        {
            _accounts = accounts;
            _executor = executor;
            _positionManager = positionManager;
            _positionSizer = positionSizer;
            _logger = logger;
        }

        public void Execute(string accountId, StrategySignal signal, decimal price, decimal atr, KinetixEngineResult context)
        {
            var acc = _accounts.First(x => x.Config.AccountId == accountId);
            if (_positionManager.HasPosition(signal.StrategyName, accountId))
            {
                //_logger.LogInformation("Account {AccountId} already has an open position for strategy {StrategyName}. Skipping execution.",
                //    accountId, signal.StrategyName);
                return;
            }
            // -------------------------------
            // GUARD
            // -------------------------------
            var guard = acc.Guard.EvaluateEntry(acc.Config, acc.State);
            if (!guard.Allowed)
            {
                _logger.LogInformation("Trade blocked for account {AccountId}: {Reason}", accountId, guard.Reason);
                return;
            }

            if (!acc.Config.Enabled || acc.State.IsPaused || acc.State.IsStopped)
            {
                _logger.LogInformation("Account {AccountId} is not active. Enabled: {Enabled}, Paused: {Paused}, Stopped: {Stopped}",
                    accountId, acc.Config.Enabled, acc.State.IsPaused, acc.State.IsStopped);
                return;
            }
            
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
            {
                _logger.LogInformation("Calculated position size {siz} for account {AccountId} is zero or negative. Skipping execution.", accountId, size);
                return;
            }
            decimal sanitizedQty = Math.Round(size, 3, MidpointRounding.ToZero);
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
                Quantity = sanitizedQty,
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
            {
                _logger.LogError("Trade execution failed for account {AccountId}: {Error}", accountId, result.Error);
                return;
            }
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