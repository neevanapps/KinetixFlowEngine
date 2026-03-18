namespace KinetixFlowEngine.Core.Prop
{
    public class GuardResult
    {
        public bool Allowed { get; set; }
        public string Reason { get; set; } = string.Empty;

        public static GuardResult Pass() => new() { Allowed = true };
        public static GuardResult Block(string reason) => new() { Allowed = false, Reason = reason };
    }

    public class PropChallengeGuard
    {
        private const decimal DAILY_DD_LIMIT = 0.04m;
        private const decimal OVERALL_DD_LIMIT = 0.10m;
        private const decimal PER_TRADE_RISK = 0.015m;

        public GuardResult EvaluateEntry(PropAccountConfig config, PropAccountState state,
            decimal entry, decimal stopLoss, decimal size)
        {
            if (!config.Enabled)
                return GuardResult.Block("Account disabled");

            if (state.IsStopped)
                return GuardResult.Block("Account stopped");

            if (state.IsPaused)
                return GuardResult.Block("Account paused (daily DD)");

            // --- Per trade risk ---
            var risk = size * Math.Abs(entry - stopLoss);
            if (risk > state.CurrentEquity * PER_TRADE_RISK)
                return GuardResult.Block("Per trade risk exceeded");

            return GuardResult.Pass();
        }

        public void UpdateEquity(PropAccountState state, decimal equity)
        {
            state.CurrentEquity = equity;

            // --- Update high watermarks ---
            if (equity > state.HighWaterMarkDaily)
                state.HighWaterMarkDaily = equity;

            if (equity > state.HighWaterMarkOverall)
                state.HighWaterMarkOverall = equity;

            // --- Drawdown calculations ---
            state.DailyDrawdownPct =
                (state.HighWaterMarkDaily - equity) / state.HighWaterMarkDaily;

            state.OverallDrawdownPct =
                (state.HighWaterMarkOverall - equity) / state.HighWaterMarkOverall;

            // --- Daily DD breach ---
            if (state.DailyDrawdownPct >= DAILY_DD_LIMIT)
                state.IsPaused = true;

            // --- Overall DD breach ---
            if (equity <= state.HighWaterMarkOverall * (1 - OVERALL_DD_LIMIT))
                state.IsStopped = true;
        }

        public void OnTradeClosed(PropAccountState state, decimal pnl)
        {
            state.CurrentEquity += pnl;

            var today = DateTime.UtcNow.Date;

            if (state.LastTradeDate != today)
            {
                state.TradingDays++;
                state.LastTradeDate = today;
            }
        }

        public void ApplyDailyReset(PropAccountState state, decimal currentEquity, DateTime nowUtc)
        {
            var today = nowUtc.Date;

            if (state.LastDailyResetUtc.Date == today)
                return;

            // ---------- RESET ----------
            state.LastDailyResetUtc = today;

            state.HighWaterMarkDaily = currentEquity;
            state.DailyDrawdownPct = 0;

            // reset pause (new day = new chance)
            state.IsPaused = false;

            // reset alert flags
            state.PauseAlertSent = false;

            // IMPORTANT: do NOT reset IsStopped (permanent)
        }
    }
}