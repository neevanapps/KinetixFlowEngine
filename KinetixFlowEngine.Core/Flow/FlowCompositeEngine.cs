using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowCompositeEngine
    {
        private readonly Ema _ema = new(5);

        private const double MaxPersistence = 10.0;

        public FlowCompositeSnapshot Calculate(FlowFeatureSnapshot f)
        {
            double persistenceNormalized =
                Math.Clamp(f.Persistence / MaxPersistence, -1.0, 1.0);

            // ------------------------------------------------
            // Balanced composite weighting
            // ------------------------------------------------

            double composite =
                0.30 * f.Imbalance +
                0.15 * f.Momentum +
                0.15 * f.Acceleration +
                0.10 * persistenceNormalized +
                0.10 * f.SizeBias +
                0.10 * f.DeltaVelocity;

            // Absorption should influence but not dominate
            composite += 0.10 * f.Absorption;

            var smoothed = _ema.Update(composite);

            return new FlowCompositeSnapshot
            {
                CompositeRaw = composite,
                CompositeSmoothed = smoothed
            };
        }
    }
}