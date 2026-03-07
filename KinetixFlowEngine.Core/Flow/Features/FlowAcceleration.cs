namespace KinetixFlowEngine.Core.Flow.Features
{
    public class FlowAcceleration
    {
        private double _previousMomentum;

        public double Calculate(double momentum)
        {
            var acceleration = momentum - _previousMomentum;
            _previousMomentum = momentum;

            return acceleration;
        }
    }
}