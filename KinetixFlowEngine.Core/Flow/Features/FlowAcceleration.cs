using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Flow.Features
{
    public class FlowAcceleration
    {
        private readonly Ema _ema = new(5);
        private double _previousMomentum;

        public double Calculate(double momentum)
        {
            var smoothedMomentum = _ema.Update(momentum);

            var acceleration = smoothedMomentum - _previousMomentum;

            _previousMomentum = smoothedMomentum;

            return acceleration;
        }
    }
}