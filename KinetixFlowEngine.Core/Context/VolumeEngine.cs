using System.Collections.Generic;

namespace KinetixFlowEngine.Core.Context
{
    public class VolumeEngine
    {
        private readonly Queue<double> _volumeWindow = new();
        private readonly int _windowSize;
        public double Sum { get; private set; }
        public int Count => _volumeWindow.Count;

        public VolumeEngine(int windowSize = 180)
        {
            _windowSize = windowSize;
        }

        public void Update(double volume)
        {
            _volumeWindow.Enqueue(volume);
            Sum += volume;

            if (_volumeWindow.Count > _windowSize)
            {
                Sum -= _volumeWindow.Dequeue();
            }
        }

        public double Average => Count == 0 ? 0 : Sum / Count;

        public bool IsVolumeExpansion(double multiplier = 1.5)
        {
            if (Count < _windowSize / 2)
                return false;

            var avg = Average;
            if (avg == 0) return false;

            // expansion if current flow significantly exceeds average flow
            return (_volumeWindow.Last() > avg * multiplier);
        }

        public void BootstrapFromCandles(List<double> volumes)
        {
            foreach (var v in volumes)
            {
                double perTick = v / 12.0;
                for (int i = 0; i < 12; i++)
                {
                    Update(perTick);
                }
            }
        }

        public void Reset()
        {
            _volumeWindow.Clear();
            Sum = 0;
        }
    }
}