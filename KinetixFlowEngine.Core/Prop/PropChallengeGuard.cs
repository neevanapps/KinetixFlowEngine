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
        private const decimal DAILY_DD_LIMIT = 4m;
        private const decimal OVERALL_DD_LIMIT = 10m;
        private const decimal PER_TRADE_RISK = .15m;

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

            // -------------------------------
            // PROTECT INITIAL STATE
            // -------------------------------
            if (state.HighWaterMarkOverall == 0)
                state.HighWaterMarkOverall = equity;

            if (state.HighWaterMarkDaily == 0)
                state.HighWaterMarkDaily = equity;

            // -------------------------------
            // UPDATE HIGH WATERMARKS
            // -------------------------------
            state.HighWaterMarkOverall = Math.Max(state.HighWaterMarkOverall, equity);
            state.HighWaterMarkDaily = Math.Max(state.HighWaterMarkDaily, equity);

            // -------------------------------
            // CALCULATE DD (%) — SINGLE SOURCE
            // -------------------------------
            state.OverallDrawdownPct =
                state.HighWaterMarkOverall == 0
                    ? 0
                    : (state.HighWaterMarkOverall - equity) / state.HighWaterMarkOverall * 100;

            state.DailyDrawdownPct =
                state.HighWaterMarkDaily == 0
                    ? 0
                    : (state.HighWaterMarkDaily - equity) / state.HighWaterMarkDaily * 100;

            // -------------------------------
            // LIMIT CHECKS (USE SAME UNIT: %)
            // -------------------------------
            if (state.DailyDrawdownPct >= DAILY_DD_LIMIT)
                state.IsPaused = true;

            if (state.OverallDrawdownPct >= OVERALL_DD_LIMIT)
                state.IsStopped = true;
        }

        public void OnTradeClosed(PropAccountState state, decimal pnl)
        {
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