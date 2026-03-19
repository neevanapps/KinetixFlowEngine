using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Prop
{
    public class AccountRuntime
    {
        public PropAccountConfig Config { get; set; } = default!;
        public PropAccountState State { get; set; } = new();
        public PropChallengeGuard Guard { get; set; } = new();
    }

    public class PropOrchestrator
    {
        private readonly List<AccountRuntime> _accounts;
        private readonly PositionManager _positionManager;
        private readonly PropAlertService _alerts;
        private readonly PropAccountStatePersistence _statePersistence;
        private readonly ITradeExecutor _executor;

        public PropOrchestrator(List<AccountRuntime> accounts, PositionManager positionManager, PropAlertService alerts, PropAccountStatePersistence statePersistence, ITradeExecutor executor)
        {
            _accounts = accounts;
            _positionManager = positionManager;
            _alerts = alerts;
            _statePersistence = statePersistence;
            _executor = executor;
        }

        public async Task UpdateEquity(decimal currentPrice)
        {
            foreach (var acc in _accounts)
            {
                var trades = _positionManager
                    .GetAllPositions()
                    .Where(t => t.AccountId == acc.Config.AccountId && !t.Closed);

                var unrealized = PropPnLCalculator.CalculateUnrealizedPnL(trades, currentPrice);
                var equity = acc.State.CurrentEquity + unrealized;
                // ---------- DAILY RESET ----------
                acc.Guard.ApplyDailyReset(acc.State, equity, DateTime.UtcNow);
                // ---------- EQUITY UPDATE ----------
                acc.Guard.UpdateEquity(acc.State, equity);

                _statePersistence.Update(acc.Config.AccountId, acc.State);

                // --- Near DD warning ---
                if (acc.State.DailyDrawdownPct >= 0.035m && acc.State.DailyDrawdownPct < 0.04m)
                {
                    await _alerts.SendNearDdWarningAsync(
                        acc.Config.AccountId,
                        acc.State.DailyDrawdownPct,
                        acc.State.OverallDrawdownPct);
                }

                // --- Paused ---
                if (acc.State.IsPaused && !acc.State.PauseAlertSent)
                {
                    acc.State.PauseAlertSent = true;

                    await _alerts.SendAccountPausedAsync(
                        acc.Config.AccountId,
                        acc.State.DailyDrawdownPct);
                }

                // --- Stopped ---
                if (acc.State.IsStopped && !acc.State.StopAlertSent)
                {
                    acc.State.StopAlertSent = true;

                    await _alerts.SendAccountStoppedAsync(
                        acc.Config.AccountId,
                        acc.State.OverallDrawdownPct);
                }
            }
        }

        public void ProcessSignal(StrategySignal signal, decimal price, decimal atr, KinetixEngineResult r)
        {
            foreach (var acc in _accounts)
            {
                if (!acc.Config.Enabled)
                    continue;

                // ---------- HARD ENFORCEMENT ----------
                if (acc.State.IsStopped)
                    continue;

                if (acc.State.IsPaused)
                    continue;

                if (acc.Config.StrategyFilter.Length > 0 &&
                    !acc.Config.StrategyFilter.Contains(signal.StrategyName))
                    continue;

                if (_positionManager.HasPosition(signal.StrategyName, acc.Config.AccountId))
                    continue;

                var stopLoss = signal.Direction == SignalDirection.Long
                    ? price - atr
                    : price + atr;

                decimal risk = acc.State.CurrentEquity * 0.005m; // 0.5%

                decimal stopDistance = Math.Abs(price - stopLoss);

                decimal size = stopDistance > 0
                    ? risk / stopDistance
                    : 0;

                if (size <= 0)
                    continue;

                // leverage cap
                var maxNotional = acc.State.CurrentEquity * acc.Config.LeverageCap;

                if (size * price > maxNotional)
                    size = maxNotional / price;

                var guard = acc.Guard.EvaluateEntry(acc.Config, acc.State, price, stopLoss, size);

                if (!guard.Allowed)
                    continue;

                decimal maxSize = (acc.State.CurrentEquity * acc.Config.LeverageCap) / price;
                size = Math.Min(size, maxSize);

                var request = new ExecutionRequest
                {
                    AccountId = acc.Config.AccountId,
                    StrategyName = signal.StrategyName,
                    Direction = signal.Direction,
                    Price = price,
                    Quantity = size,
                    StopLoss = stopLoss,
                    TakeProfit = signal.Direction == SignalDirection.Long
                                ? price + (price - stopLoss)
                                : price - (stopLoss - price)
                };

                var result = _executor.ExecuteAsync(request).Result;

                if (!result.Success)
                    continue;

                // use FILLED price (important)
                _positionManager.TryEnterTrade(
                    signal,
                    result.FilledPrice,
                    (double)atr,
                    r,
                    result.FilledQuantity,
                    acc.Config.AccountId);
            }
        }
    }
}