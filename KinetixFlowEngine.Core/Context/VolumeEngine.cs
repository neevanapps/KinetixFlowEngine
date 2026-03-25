using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Context
{
    public class VolumeState
    {
        public double Sum { get; set; }
        public double[] Window { get; set; } = Array.Empty<double>();
        public int Count { get; set; }
    }

    public class VolumeEngine
    {
        private readonly Queue<double> _volumeWindow = new();
        private readonly int _windowSize = 900;           // 15 minutes @ 1s updates
        private readonly Ema _smoothedAvg = new Ema(30); // light smoothing for stability

        public double Sum { get; private set; }
        public int Count => _volumeWindow.Count;

        public void Update(double volume)
        {
            _volumeWindow.Enqueue(volume);
            Sum += volume;

            if (_volumeWindow.Count > _windowSize)
                Sum -= _volumeWindow.Dequeue();

            _smoothedAvg.Update(Average);
        }

        public double Average => Count == 0 ? 0 : Sum / Count;

        public bool IsVolumeExpansion(double multiplier = 1.5)
        {
            if (Count < _windowSize / 3) return false;   // need enough history
            if (Average == 0) return false;

            // Now stable and less noisy thanks to smoothed average
            return _volumeWindow.Last() > _smoothedAvg.Value * multiplier;
        }

        // Restart-safe state (critical for your Slow EMA boost)
        public VolumeState GetState()
        {
            return new VolumeState
            {
                Sum = Sum,
                Window = _volumeWindow.ToArray(),
                Count = Count
            };
        }

        public void Restore(VolumeState state)
        {
            _volumeWindow.Clear();
            Sum = state.Sum;

            foreach (var v in state.Window)
                _volumeWindow.Enqueue(v);

            // Re-warm smoothed average
            for (int i = 0; i < 30; i++)
                _smoothedAvg.Update(Average);
        }

        public void Reset()
        {
            _volumeWindow.Clear();
            Sum = 0;
        }
    }
}