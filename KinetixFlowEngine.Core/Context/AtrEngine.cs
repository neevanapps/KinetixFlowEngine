namespace KinetixFlowEngine.Core.Context
{
    public class AtrEngine
    {
        private readonly Queue<double> _ranges = new();
        private const int Period = 14;

        public double Update(double high, double low)
        {
            double range = high - low;

            _ranges.Enqueue(range);

            if (_ranges.Count > Period)
                _ranges.Dequeue();

            return _ranges.Average();
        }
    }
}