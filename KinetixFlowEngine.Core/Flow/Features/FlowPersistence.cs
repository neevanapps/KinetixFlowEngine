namespace KinetixFlowEngine.Core.Flow.Features
{
    public class FlowPersistence
    {
        private double _score;

        public double Calculate(double imbalance)
        {
            const double threshold = 0.15;

            if (imbalance > threshold)
                _score += 1;

            else if (imbalance < -threshold)
                _score -= 1;

            else
                _score *= 0.7; // decay

            return _score;
        }
    }
}