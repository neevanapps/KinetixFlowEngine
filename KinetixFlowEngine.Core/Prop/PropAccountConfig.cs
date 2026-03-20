namespace KinetixFlowEngine.Core.Prop
{
    public enum PropAccountStage
    {
        ChallengePhase1,
        ChallengePhase2,
        Funding
    }
    public enum DailyDdMode
    {
        FromStartOfDay = 0,
        FromHighWatermark = 1
    }
    public class PropAccountConfig
    {
        public string AccountId { get; set; } = default!;
        public PropAccountStage Stage { get; set; }

        public decimal StartingCapital { get; set; }
        public decimal LeverageCap { get; set; } = 1.0m;

        public string ApiKey { get; set; } = default!;
        public string ApiSecret { get; set; } = default!;

        public string[] StrategyFilter { get; set; } = Array.Empty<string>();

        public decimal ProfitSharePercentage { get; set; } = 0m;

        public bool Enabled { get; set; } = true;

        public DailyDdMode DailyDdMode { get; set; } = DailyDdMode.FromHighWatermark;
    }
}