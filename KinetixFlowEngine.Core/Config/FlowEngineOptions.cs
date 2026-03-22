namespace KinetixFlowEngine.Core.Config
{
    public class FlowEngineOptions
    {
        public int ScoreIntervalMs { get; set; } = 1000;
        public int SnapshotIntervalSeconds { get; set; } = 60;
        public int RestartToleranceMinutes { get; set; } = 10;
        public int NormalizationHours { get; set; } = 12;
        public int EngineCycleSeconds { get; set; } = 5;
        public decimal FeeRate { get; set; } = 0.0002m; // Bybit maker
        public int StabilizationMinutesBeforeTrading { get; set; } = 15;
    }
}