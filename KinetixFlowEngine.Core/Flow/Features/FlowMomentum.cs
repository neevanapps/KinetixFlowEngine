namespace KinetixFlowEngine.Core.Flow.Features
{
    public class FlowMomentum
    {
        private double _previous;

        public double Calculate(double imbalance)
        {
            var momentum = imbalance - _previous;
            _previous = imbalance;

            return momentum;
        }
    }
}