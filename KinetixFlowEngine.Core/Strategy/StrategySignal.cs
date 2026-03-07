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
    }
}