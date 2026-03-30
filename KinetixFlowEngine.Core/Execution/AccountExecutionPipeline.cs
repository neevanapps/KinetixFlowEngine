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
        Task Execute(string accountId, StrategySignal signal, decimal price, decimal atr, KinetixEngineResult context);
    }

    public class AccountExecutionPipeline : IAccountExecutionPipeline
    {
        private readonly PropAccountRuntimeManager _accounts;
        private readonly ITradeExecutor _executor;
        private readonly PositionManager _positionManager;
        private readonly IPositionSizer _positionSizer;
        private readonly ILogger<AccountExecutionPipeline> _logger;
        private readonly ExecutionGuard _guard;

        public AccountExecutionPipeline(
            PropAccountRuntimeManager accounts,
            ITradeExecutor executor,
            PositionManager positionManager,
            IPositionSizer positionSizer,
            ILogger<AccountExecutionPipeline> logger,
            ExecutionGuard guard)
        {
            _accounts = accounts;
            _executor = executor;
            _positionManager = positionManager;
            _positionSizer = positionSizer;
            _logger = logger;
            _guard = guard;
        }

        public async Task Execute(string accountId, StrategySignal signal, decimal price, decimal atr, KinetixEngineResult context)
        {
            var acc = _accounts.Accounts.First(x => x.Config.AccountId == accountId);
            if (await _positionManager.HasPosition(signal.StrategyName, accountId) || await _guard.IsBusy(accountId))
            {
                //_logger.LogInformation("Account {AccountId} already has an open position for strategy {StrategyName}. Skipping execution.",
                //    accountId, signal.StrategyName);
                return;
            }
            if (await _guard.IsBusy(accountId))
            {
                _logger.LogInformation("Skipping execution, order in-flight for {AccountId}", accountId);
                return;
            }

            if (!await _guard.TryEnter(accountId))
            {
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
                StopLoss = stopLoss
            };

            // -------------------------------
            // EXECUTE ORDER (CRITICAL)
            // -------------------------------
            try
            {
                var result = await _executor.ExecuteAsync(request);

                if (!result.Success)
                {
                    _logger.LogError("Trade execution failed for account {AccountId}: {Error}", accountId, result.Error);
                    return;
                }

                _positionManager.TryEnterTrade(
                    signal,
                    result.FilledPrice,
                    (double)atr,
                    context,
                    result.FilledQuantity,
                    accountId,
                    result.OrderId);
            }
            finally
            {
                _guard.Exit(accountId);
            }
        }
    }
}