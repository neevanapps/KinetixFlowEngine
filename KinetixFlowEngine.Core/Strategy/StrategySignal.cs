namespace KinetixFlowEngine.Core.Strategy
{
    public class StrategySignal
    {
        public string StrategyName { get; set; } = "";

        public SignalDirection Direction { get; set; }

        public double Confidence { get; set; }

        public bool EnterOnlyAtFairPrice { get; set; }

        public bool NotifyThroughTelegram { get; set; }

        public decimal? SuggestedEntryPrice { get; set; }

        public bool FairPriceApproved { get; set; }

        public bool IsVolumeBased { get; set; }

        public bool ExitSignal { get; set; }
        public decimal RiskPercent { get; set; } = 0.01m; // default 1%
        public List<string> TargetAccountIds { get; set; } = new List<string>();

        public StrategySignal Clone()
        {
            return new StrategySignal
            {
                StrategyName = StrategyName,
                Direction = Direction,
                Confidence = Confidence,
                EnterOnlyAtFairPrice = EnterOnlyAtFairPrice,
                NotifyThroughTelegram = NotifyThroughTelegram,
                SuggestedEntryPrice = SuggestedEntryPrice,
                FairPriceApproved = FairPriceApproved,
                IsVolumeBased = IsVolumeBased,
                ExitSignal = ExitSignal,
                RiskPercent = RiskPercent,
                TargetAccountIds = new List<string>(TargetAccountIds)
            };
        }
    }
}