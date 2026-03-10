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

            double composite = 0.25 * f.Imbalance + 0.15 * f.Momentum + 0.15 * f.Acceleration + 0.15 * persistenceNormalized + 0.10 * f.SizeBias + 0.10 * f.DeltaVelocity;
            composite += 0.30 * f.Absorption;

            if (f.Absorption != 0)
                composite += 0.15 * f.Absorption;

            var smoothed = _ema.Update(composite);

            return new FlowCompositeSnapshot
            {
                CompositeRaw = composite,
                CompositeSmoothed = smoothed
            };
        }
    }
}