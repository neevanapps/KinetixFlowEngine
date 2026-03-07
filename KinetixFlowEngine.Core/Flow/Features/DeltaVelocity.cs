namespace KinetixFlowEngine.Core.Flow.Features
{
    public class DeltaVelocity
    {
        private double _previousImbalance;

        public double Calculate(double imbalance)
        {
            double velocity = imbalance - _previousImbalance;
            _previousImbalance = imbalance;
            return velocity;
        }
    }
}