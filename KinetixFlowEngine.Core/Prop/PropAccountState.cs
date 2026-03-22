namespace KinetixFlowEngine.Core.Prop
{
    public class PropAccountState
    {
        public decimal CurrentEquity { get; set; }
        public decimal HighWaterMarkDaily { get; set; }
        public decimal HighWaterMarkOverall { get; set; }
        public decimal DailyDrawdownPct { get; set; }
        public decimal OverallDrawdownPct { get; set; }
        public int TradingDays { get; set; }
        public bool IsPaused { get; set; }
        public bool IsStopped { get; set; }
        public bool PauseAlertSent { get; set; }
        public bool StopAlertSent { get; set; }
        public DateTime LastTradeDate { get; set; } = DateTime.MinValue;
        public DateTime LastDailyResetUtc { get; set; } = DateTime.MinValue;
        public decimal DayStartEquity { get; set; }
        public DateTime? LastBalanceSyncUtc { get; set; }  // optional

        public void Initialize(decimal startingCapital)
        {
            CurrentEquity = startingCapital;
            HighWaterMarkDaily = startingCapital;
            HighWaterMarkOverall = startingCapital;
        }
    }
}