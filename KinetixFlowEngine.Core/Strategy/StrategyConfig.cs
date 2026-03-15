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

        public decimal Target1Atr { get; set; } = 2;

        public decimal Target1SizePercent { get; set; } = 50;

        public decimal Target2Atr { get; set; } = 4;

        public decimal Target2SizePercent { get; set; } = 50;

        public decimal StopLossAtr { get; set; } = 1;

        public decimal TrailingStopAtr { get; set; } = 1;

        public bool VolumeBased { get; set; }
    }
}