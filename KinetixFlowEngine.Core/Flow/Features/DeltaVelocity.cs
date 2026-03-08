using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Flow.Features
{
    public class DeltaVelocity
    {
        private readonly Ema _ema = new(5);
        private double _previous;

        public double Calculate(double imbalance)
        {
            var smoothed = _ema.Update(imbalance);

            var velocity = smoothed - _previous;

            _previous = smoothed;

            return velocity;
        }
    }
}