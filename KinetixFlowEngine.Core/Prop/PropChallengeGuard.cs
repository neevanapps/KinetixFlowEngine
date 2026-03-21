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
        private const decimal DAILY_DD_LIMIT = 5m;     // 5%
        private const decimal OVERALL_DD_LIMIT = 10m;   // 10%

        public GuardResult EvaluateEntry(PropAccountConfig config, PropAccountState state)
        {
            if (!config.Enabled)
                return GuardResult.Block("Account disabled");

            if (state.IsStopped)
                return GuardResult.Block("Account stopped");

            if (state.IsPaused)
                return GuardResult.Block("Account paused (daily DD)");

            if (state.DailyDrawdownPct >= DAILY_DD_LIMIT)
                return GuardResult.Block("Daily DD limit");

            if (state.OverallDrawdownPct >= OVERALL_DD_LIMIT)
                return GuardResult.Block("Overall DD limit");

            return GuardResult.Pass();
        }
    }
}