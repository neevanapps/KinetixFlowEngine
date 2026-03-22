using KinetixFlowEngine.Core.Persistence;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Prop
{
    public interface IEquityEngine
    {
        void UpdateAll(decimal currentPrice);
    }

    public class EquityEngine : IEquityEngine
    {
        private readonly PropAccountRuntimeManager _accounts;
        private readonly PositionManager _positions;
        private readonly AccountStateEngine _stateEngine;
        private readonly ILogger<EquityEngine> _logger;
        private readonly TradeJournalRecorder _tradeJournal;
        private readonly PropAccountStatePersistence _accountStatePersistence;

        public EquityEngine(
            PropAccountRuntimeManager accounts,
            PositionManager positions,
            AccountStateEngine stateEngine,
            TradeJournalRecorder tradeJournal,
            PropAccountStatePersistence accountStatePersistence,
            ILogger<EquityEngine> logger)
        {
            _accounts = accounts;
            _positions = positions;
            _stateEngine = stateEngine;
            _tradeJournal = tradeJournal;
            _accountStatePersistence = accountStatePersistence;
            _logger = logger;
        }
        public void UpdateAll(decimal currentPrice)
        {
            var now = DateTime.UtcNow;

            foreach (var acc in _accounts.Accounts)
            {
                var trades = _positions
                    .GetAllPositions()
                    .Where(t => t.AccountId == acc.Config.AccountId && !t.Closed);

                decimal floatingPnL = 0;

                if (acc.State.CurrentEquity > acc.State.DayStartEquity * 2)
                {
                    acc.State.IsStopped = true;
                    _logger.LogCritical("EQUITY CORRUPTION DETECTED");
                }

                foreach (var t in trades)
                {
                    floatingPnL += t.Direction == SignalDirection.Long
                        ? (currentPrice - t.EntryPrice) * t.RemainingSize
                        : (t.EntryPrice - currentPrice) * t.RemainingSize;
                }

                var equity = acc.State.CurrentEquity + floatingPnL;

                // ✅ PASS CONFIG
                _stateEngine.UpdateState(acc.Config, acc.State, equity, now);
            }
        }

    }
}