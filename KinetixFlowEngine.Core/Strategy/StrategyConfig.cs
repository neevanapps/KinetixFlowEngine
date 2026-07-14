namespace KinetixFlowEngine.Core.Strategy
{
    public class StrategyConfig
    {
        public string StrategyName { get; set; } = "";

        public bool Enabled { get; set; } = true;

        public bool EnterOnlyAtFairPrice { get; set; }

        public bool NotifyThroughTelegram { get; set; }

        public double MinConfidence { get; set; } = 1.5;

        public int CooldownSeconds { get; set; } = 0;

        public decimal PositionSize { get; set; } = 1;

        // Used for journal/reporting only in this minimal phase.
        // It does not change live or SIM order quantity yet.
        public decimal Leverage { get; set; } = 1;

        public decimal Target1Atr { get; set; } = 2;

        // Percentage distance from entry. Example: 0.5 means 0.5%, not 50%.
        public decimal Target1Percent { get; set; } = 1;

        public decimal Target1SizePercent { get; set; } = 50;

        public decimal Target2Atr { get; set; } = 4;

        public decimal Target2SizePercent { get; set; } = 50;

        public decimal StopLossAtr { get; set; } = 3;

        // Percentage distance from entry. Example: 1 means 1%.
        public decimal StopLossPercent { get; set; } = 1;

        public decimal TrailingStopAtr { get; set; } = 1;

        public bool CanReenterSameDirection { get; set; } = false;

        // Example: 0.15 means +/-0.15% around the previous entry price.
        public decimal ReEntryRangePercent { get; set; } = 0.15m;

        public bool ReEntryOnlyAfterProfit { get; set; } = true;

        public bool ReEntryOnlyAfterTargetHit { get; set; } = true;

        public bool VolumeBased { get; set; }
        public List<string> AccountIds { get; set; } = new List<string>();
    }
}