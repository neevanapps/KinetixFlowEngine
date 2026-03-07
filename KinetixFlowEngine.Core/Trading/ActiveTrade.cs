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

        public bool NotifyThroughTelegram { get; set; }
    }
}