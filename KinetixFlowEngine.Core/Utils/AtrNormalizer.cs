using System;
using System.Collections.Generic;
using System.Linq;

namespace KinetixFlowEngine.Core.Utils
{
    public class AtrNormalizer
    {
        private readonly int _windowSize;
        private readonly Queue<double> _values = new();
        private double _sum = 0;
        private double _sumSq = 0;

        public double Mean { get; private set; } = 0;
        public double StdDev { get; private set; } = 1;

        public AtrNormalizer(int windowSize = 1440)
        {
            _windowSize = windowSize;
        }

        public void Update(double atr)
        {
            // avoid duplicate inserts (1-min cadence)
            if (_values.Count > 0 && Math.Abs(_values.Last() - atr) < 0.0001)
                return;

            _values.Enqueue(atr);
            _sum += atr;
            _sumSq += atr * atr;

            if (_values.Count > _windowSize)
            {
                var old = _values.Dequeue();
                _sum -= old;
                _sumSq -= old * old;
            }

            if (_values.Count < 30) return;

            Mean = _sum / _values.Count;
            double variance = (_sumSq / _values.Count) - (Mean * Mean);
            StdDev = Math.Sqrt(Math.Max(variance, 1e-6));
        }

        // returns 0–1 regime
        public double GetNormalized(double currentAtr)
        {
            if (StdDev < 1e-6) return 0.5;

            double z = (currentAtr - Mean) / StdDev;
            z = Math.Clamp(z, -2.0, 2.0);

            // map [-2,2] → [0,1]
            return (z + 2.0) / 4.0;
        }

        // returns scale for score normalization
        public double GetScale(double normalized)
        {
            // 0 → 0.7, 1 → 1.6
            double scale = 0.7 + normalized * 0.9;
            return Math.Clamp(scale, 0.7, 1.6);
        }

        public AtrNormalizerState GetState()
        {
            return new AtrNormalizerState
            {
                Values = _values.ToArray(),
                Sum = _sum,
                SumSq = _sumSq
            };
        }

        public void Restore(AtrNormalizerState state)
        {
            _values.Clear();
            _sum = state.Sum;
            _sumSq = state.SumSq;

            foreach (var v in state.Values)
                _values.Enqueue(v);

            if (_values.Count > 0)
            {
                Mean = _sum / _values.Count;
                double variance = (_sumSq / _values.Count) - (Mean * Mean);
                StdDev = Math.Sqrt(Math.Max(variance, 1e-6));
            }
        }
    }

    public class AtrNormalizerState
    {
        public double[] Values { get; set; } = Array.Empty<double>();
        public double Sum { get; set; }
        public double SumSq { get; set; }
    }
}