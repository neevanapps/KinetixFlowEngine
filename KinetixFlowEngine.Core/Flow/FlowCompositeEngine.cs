using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowCompositeEngine
    {
        private readonly Ema _ema = new(5);

        private const double MaxPersistence = 10.0;

        public FlowCompositeSnapshot Calculate(FlowFeatureSnapshot f)
        {
            double composite =
                  0.30 * f.Imbalance
                + 0.15 * Math.Tanh(f.Persistence / 5.0)
                + 0.20 * f.Momentum
                + 0.10 * f.Acceleration
                + 0.15 * f.Absorption
                + 0.10 * f.SizeBias;

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