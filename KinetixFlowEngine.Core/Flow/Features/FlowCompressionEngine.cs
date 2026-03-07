namespace KinetixFlowEngine.Core.Flow.Features
{
    public class FlowCompressionEngine
    {
        private readonly Queue<double> _imbalanceHistory = new();
        private const int Window = 10;

        public double Update(double imbalance)
        {
            _imbalanceHistory.Enqueue(Math.Abs(imbalance));

            if (_imbalanceHistory.Count > Window)
                _imbalanceHistory.Dequeue();

            if (_imbalanceHistory.Count < Window)
                return 0;

            var avg = _imbalanceHistory.Average();

            return 1.0 - avg;
        }
    }
}