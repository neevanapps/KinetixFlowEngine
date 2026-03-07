namespace KinetixFlowEngine.Core.Flow
{
    public class FlowRegimeSnapshot
    {
        public FlowRegime Regime { get; set; }

        public double Score { get; set; }

        public bool Bullish => Score > 0;

        public bool Bearish => Score < 0;
    }
}