using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Trading
{
    public class ActiveTrade
    {
        public string StrategyName { get; set; } = "";

        public SignalDirection Direction { get; set; }

        public decimal EntryPrice { get; set; }

        public decimal StopLoss { get; set; }

        public decimal Target1 { get; set; }

        public decimal TrailingStop { get; set; }

        public decimal InitialSize { get; set; }

        public decimal RemainingSize { get; set; }

        public bool Target1Hit { get; set; }

        public long EntryTimeMs { get; set; }

        public decimal MaxPrice { get; set; }

        public decimal MinPrice { get; set; }

        public bool NotifyThroughTelegram { get; set; }

        public bool Closed { get; set; }
        public double EntryScoreZ { get; set; }
        public double EntryVelocityZ { get; set; }
        public double EntryImbalanceZ { get; set; }
        public double EntryCompressionZ { get; set; }
        public double EntryATR { get; set; }
        public double EntryER { get; set; }
        public string EntryFlowState { get; set; } = "";
    }
}