namespace KinetixFlowEngine.Core.Flow.Features
{
    public class FlowPersistence
    {
        private int _bullCount;
        private int _bearCount;

        public double Calculate(double imbalance)
        {
            if (imbalance > 0.2)
            {
                _bullCount++;
                _bearCount = 0;
            }
            else if (imbalance < -0.2)
            {
                _bearCount++;
                _bullCount = 0;
            }
            else
            {
                _bullCount = 0;
                _bearCount = 0;
            }

            return _bullCount - _bearCount;
        }
    }
}