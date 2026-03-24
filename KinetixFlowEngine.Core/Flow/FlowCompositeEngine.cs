using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowCompositeEngine
    {
        private readonly Ema _ema = new(5);

        private const double MaxPersistence = 10.0;

        public FlowCompositeSnapshot Calculate(FlowFeatureSnapshot f)
        {
            double persistenceNormalized = Math.Clamp(f.Persistence / MaxPersistence, -1.0, 1.0);

            // Increased weights on the most whale-relevant signals
            double imbalanceComponent = 0.40 * f.Imbalance;     // was 0.35
            double persistenceComponent = 0.30 * persistenceNormalized; // was 0.25
            double momentumComponent = 0.18 * f.Momentum;      // was 0.15

            // Secondary (still important but not dominant)
            double accelerationComponent = 0.10 * f.Acceleration;  // was 0.08
            double velocityComponent = 0.12 * f.DeltaVelocity; // was 0.10

            // Institutional / whale signals — boosted
            double sizeBiasComponent = 0.08 * f.SizeBias;      // was 0.05
            double absorptionComponent = 0.10 * f.Absorption;    // was 0.07

            double composite =
                            0.30 * f.Imbalance +
                            0.22 * persistenceNormalized +
                            0.15 * f.Momentum +
                            0.10 * f.Acceleration +
                            0.10 * f.DeltaVelocity +
                            0.06 * f.SizeBias +
                            0.07 * f.Absorption;

            // Wider clamp so scoreZ can actually reach ±2.5+ when whales are active
            composite = Math.Clamp(composite, -2.5, 2.5);

            var smoothed = _ema.Update(composite);

            return new FlowCompositeSnapshot
            {
                CompositeRaw = composite,
                CompositeSmoothed = smoothed
            };
        }
    }
}