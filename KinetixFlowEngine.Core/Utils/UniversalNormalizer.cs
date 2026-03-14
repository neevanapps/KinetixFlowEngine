using System.Text.Json.Serialization;

namespace KinetixFlowEngine.Core.Utils
{
    public class UniversalNormalizer
    {
        private readonly Queue<double> _window = new();
        private readonly int _maxSamples;

        private double _mean;
        private double _m2;
        private int _count;

        public bool IsReady => _count >= 100;

        public UniversalNormalizer(int maxSamples)
        {
            _maxSamples = maxSamples;
        }

        public double Update(double value)
        {
            _window.Enqueue(value);
            if (_window.Count > _maxSamples)
                _window.Dequeue();

            _count++;

            double delta = value - _mean;
            _mean += delta / _count;
            _m2 += delta * (value - _mean);

            if (_count < 100)
                return 0;

            double variance = _m2 / (_count - 1);
            double std = Math.Sqrt(variance);

            if (std == 0)
                return 0;

            return (value - _mean) / std;
        }

        public NormalizerState GetState()
        {
            return new NormalizerState
            {
                Values = _window.ToList(),
                Mean = _mean,
                M2 = _m2,
                Count = _count
            };
        }

        public void Restore(NormalizerState state)
        {
            _window.Clear();

            foreach (var v in state.Values)
                _window.Enqueue(v);

            _mean = state.Mean;
            _m2 = state.M2;
            _count = state.Count;
        }
    }

    public class NormalizerState
    {
        public List<double> Values { get; set; } = new();

        public double Mean { get; set; }

        public double M2 { get; set; }

        public int Count { get; set; }
    }
}