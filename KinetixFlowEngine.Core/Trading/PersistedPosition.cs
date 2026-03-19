using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Trading
{
    public class PersistedPosition
    {
        public string AccountId { get; set; } = default!;
        public string StrategyName { get; set; } = default!;
        public SignalDirection Direction { get; set; }

        public decimal EntryPrice { get; set; }
        public decimal StopLoss { get; set; }
        public decimal Target1 { get; set; }

        public decimal Size { get; set; }

        public long EntryTimeMs { get; set; }

        public bool Target1Hit { get; set; }

        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }

        public double EntryScoreZ { get; set; }
        public double EntryVelocityZ { get; set; }
        public double EntryImbalanceZ { get; set; }
        public double EntryCompressionZ { get; set; }
        public double EntryATR { get; set; }
        public double EntryER { get; set; }

        public string EntryFlowState { get; set; } = default!;

        public double EntryPriceTrend { get; set; }
        public double EntryScoreTrend { get; set; }
        public bool EntryAlertSent { get; set; }   // ✅ ADD
        public bool MovedToBreakeven { get; set; } // ✅ ADD
        public decimal RemainingSize { get; set; } // ✅ ADD
        public decimal TrailingStop { get; set; }  // ✅ ADD
    }
}