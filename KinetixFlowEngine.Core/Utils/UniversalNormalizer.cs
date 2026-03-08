using System.Text.Json.Serialization;

namespace KinetixFlowEngine.Core.Utils
{
    public class UniversalNormalizer
    {
        private readonly Queue<double> _values = new();
        public int MaxSamples { get; }
        public int Count => _values.Count;
        public bool IsReady => _values.Count >= 20;

        public UniversalNormalizer(int maxSamples)
        {
            MaxSamples = maxSamples;
        }

        public double Update(double value)
        {
            _values.Enqueue(value);

            if (_values.Count > MaxSamples)
                _values.Dequeue();

            if (_values.Count < 100)
                return 0;

            var mean = _values.Average();

            double variance = 0;

            foreach (var v in _values)
                variance += (v - mean) * (v - mean);

            variance /= _values.Count;

            var std = Math.Sqrt(variance);

            if (std == 0)
                return 0;

            return (value - mean) / std;
        }

        public NormalizerState GetState()
        {
            return new NormalizerState
            {
                Values = _values.ToList()
            };
        }

        public void Restore(NormalizerState state)
        {
            _values.Clear();

            foreach (var v in state.Values)
                _values.Enqueue(v);
        }
    }

    public class NormalizerState
    {
        public List<double> Values { get; set; } = new();
    }
}