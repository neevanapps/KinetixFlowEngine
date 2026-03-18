using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Prop
{
    public class PropAlertService
    {
        private readonly TelegramService _telegram;

        // simple de-dup cache per account per day
        private readonly Dictionary<string, bool> _dailyWarnSent = new();
        private readonly Dictionary<string, bool> _overallWarnSent = new();

        public PropAlertService(TelegramService telegram)
        {
            _telegram = telegram;
        }

        public async Task SendEntryAsync(ActiveTrade trade, decimal equity, decimal dailyDd, decimal overallDd)
        {
            await _telegram.SendMessageAsync(
            $"""
            ENTRY | {trade.AccountId} | {trade.Direction} | {trade.StrategyName}

            Entry={trade.EntryPrice:F1}  SL={trade.StopLoss:F1}  TP1={trade.Target1:F1}
            Size={trade.InitialSize:F4}

            Equity={equity:F0}
            DailyDD={dailyDd:P2}  OverallDD={overallDd:P2}
            """);
        }

        public async Task SendExitAsync(ActiveTrade trade, decimal exitPrice, decimal pnl, decimal fee, decimal equity, decimal dailyDd, decimal overallDd)
        {
            await _telegram.SendMessageAsync(
            $"""
            EXIT | {trade.AccountId} | {trade.Direction} | {trade.StrategyName}

            Entry={trade.EntryPrice:F1}  Exit={exitPrice:F1}
            PnL={pnl:F1}  Fee={fee:F0}

            Equity={equity:F0}
            DailyDD={dailyDd:P2}  OverallDD={overallDd:P2}
            """);
        }

        public async Task SendTarget1Async(ActiveTrade trade)
        {
            await _telegram.SendMessageAsync(
            $"""
            TP1 | {trade.AccountId} | {trade.Direction} | {trade.StrategyName}

            Entry={trade.EntryPrice:F1}
            Remaining={trade.RemainingSize:P0}
            """);
        }

        public async Task SendNearDdWarningAsync(string accountId, decimal dailyDd, decimal overallDd)
        {
            // one warning per account per session/day (simple guard)
            if (_dailyWarnSent.ContainsKey(accountId))
                return;

            _dailyWarnSent[accountId] = true;

            await _telegram.SendMessageAsync(
            $"""
            ⚠️ DD WARNING | {accountId}

            DailyDD={dailyDd:P2}
            OverallDD={overallDd:P2}
            """);
        }

        public async Task SendAccountPausedAsync(string accountId, decimal dailyDd)
        {
            await _telegram.SendMessageAsync(
            $"""
            ⛔ ACCOUNT PAUSED | {accountId}

            DailyDD={dailyDd:P2}
            """);
        }

        public async Task SendAccountStoppedAsync(string accountId, decimal overallDd)
        {
            await _telegram.SendMessageAsync(
            $"""
            ⛔ ACCOUNT STOPPED | {accountId}

            OverallDD={overallDd:P2}
            """);
        }

        public void ResetDailyFlags()
        {
            _dailyWarnSent.Clear();
        }
    }
}