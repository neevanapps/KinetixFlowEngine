using System.Text.Json.Serialization;

namespace KinetixFlowEngine.Core.Utils
{
    public class UniversalNormalizer
    {
        private double _mean = 0;
        private double _variance = 1;
        private bool _initialized = false;

        private int _warmupCount = 0;
        private const int WARMUP = 100;

        public bool IsReady => _warmupCount >= WARMUP;

        // fallback alpha (if not provided)
        private const double DEFAULT_ALPHA = 0.05;

        public UniversalNormalizer(int maxSamples)
        {
            // maxSamples ignored now (kept for compatibility)
        }

        public double Update(double value)
        {
            return Update(value, DEFAULT_ALPHA);
        }

        public double Update(double value, double alpha)
        {
            // clamp alpha for safety
            alpha = Math.Clamp(alpha, 0.01, 0.3);

            if (!_initialized)
            {
                _mean = value;
                _variance = 1;
                _initialized = true;
                return 0;
            }

            _warmupCount++;

            // --- EMA mean ---
            double prevMean = _mean;
            _mean = alpha * value + (1 - alpha) * _mean;

            // --- EMA variance ---
            double diff = value - prevMean;
            _variance = alpha * (diff * diff) + (1 - alpha) * _variance;

            if (_warmupCount < WARMUP)
                return 0;

            double std = Math.Sqrt(_variance);

            if (std < 1e-8)
                return 0;

            return (value - _mean) / std;
        }

        public NormalizerState GetState()
        {
            return new NormalizerState
            {
                Mean = _mean,
                Variance = _variance,
                WarmupCount = _warmupCount
            };
        }

        public void Restore(NormalizerState state)
        {
            _mean = state.Mean;
            _variance = state.Variance;
            _warmupCount = state.WarmupCount;
            _initialized = true;
        }
    }

    public class NormalizerState
    {
        public double Mean { get; set; }
        public double Variance { get; set; }
        public int WarmupCount { get; set; }
    }

    public static class AdaptiveAlpha
    {
        public static double Compute(double atr, double er)
        {
            double atrNorm = Math.Clamp(atr / 100.0, 0.5, 2.0);

            double regime = (atrNorm * 0.7) + (er * 0.3);

            double minWindow = 20;
            double maxWindow = 100;

            double window = maxWindow - (regime * (maxWindow - minWindow));

            double alpha = 2.0 / (window + 1.0);

            return Math.Clamp(alpha, 0.02, 0.3);
        }
    }
}