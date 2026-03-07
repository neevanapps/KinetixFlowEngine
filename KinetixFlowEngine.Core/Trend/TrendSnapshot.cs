namespace KinetixFlowEngine.Core.Trend
{
    public class TrendSnapshot
    {
        public FlowTrend Trend { get; set; }

        public double FastEma { get; set; }

        public double SlowEma { get; set; }

        public double Spread => FastEma - SlowEma;
    }
}