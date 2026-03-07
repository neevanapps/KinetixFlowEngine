namespace KinetixFlowEngine.Core.Flow
{
    public class FlowFeatureSnapshot
    {
        public double Imbalance { get; set; }

        public double Momentum { get; set; }

        public double Acceleration { get; set; }

        public double Persistence { get; set; }

        public double SizeBias { get; set; }

        public double Absorption { get; set; }

        public double DeltaVelocity { get; set; }

        public double Compression { get; set; }

        public double Exhaustion { get; set; }
    }
}