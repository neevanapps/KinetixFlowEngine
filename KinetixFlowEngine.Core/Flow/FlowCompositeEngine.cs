using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowCompositeEngine
    {
        private readonly Ema _ema = new(5);

        private const double MaxPersistence = 10.0;

        public FlowCompositeSnapshot Calculate(FlowFeatureSnapshot f)
        {
            //------------------------------------------------
            // Normalize persistence
            //------------------------------------------------

            double persistenceNormalized =
                Math.Clamp(f.Persistence / MaxPersistence, -1.0, 1.0);

            //------------------------------------------------
            // Core flow structure
            //------------------------------------------------

            double imbalanceComponent = 0.35 * f.Imbalance;
            double persistenceComponent = 0.25 * persistenceNormalized;

            //------------------------------------------------
            // Secondary flow structure
            //------------------------------------------------

            double momentumComponent = 0.15 * f.Momentum;

            //------------------------------------------------
            // Noise-prone signals (reduced influence)
            //------------------------------------------------

            double accelerationComponent = 0.08 * f.Acceleration;
            double velocityComponent = 0.10 * f.DeltaVelocity;

            //------------------------------------------------
            // Institutional signals
            //------------------------------------------------

            double sizeBiasComponent = 0.05 * f.SizeBias;
            double absorptionComponent = 0.07 * f.Absorption;

            //------------------------------------------------
            // Composite calculation
            //------------------------------------------------

            double composite =
                imbalanceComponent +
                persistenceComponent +
                momentumComponent +
                accelerationComponent +
                velocityComponent +
                sizeBiasComponent +
                absorptionComponent;

            //------------------------------------------------
            // Anti-spike clamp
            //------------------------------------------------

            composite = Math.Clamp(composite, -1.5, 1.5);
            composite = Math.Tanh(composite);
            //------------------------------------------------
            // EMA smoothing
            //------------------------------------------------

            var smoothed = _ema.Update(composite);

            return new FlowCompositeSnapshot
            {
                CompositeRaw = composite,
                CompositeSmoothed = smoothed
            };
        }
    }
}