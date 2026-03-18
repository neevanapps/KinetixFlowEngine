using System.Collections.Generic;

namespace KinetixFlowEngine.Core.Context
{
    public class VolumeEngine
    {
        private readonly Queue<double> _volumeWindow = new();
        private readonly Queue<double> _CumulativeWindow = new();

        private readonly int _windowSize;
        private readonly int _CumulativeSize;

        public double Sum;
        public double CumulativeSum;

        public VolumeEngine(int windowSize = 180)
        {
            _windowSize = windowSize;
            _CumulativeSize = 12;
        }

        public void Update(double volume)
        {
            _volumeWindow.Enqueue(volume);
            _CumulativeWindow.Enqueue(volume);
            Sum += volume;
            CumulativeSum += volume;

            if (_volumeWindow.Count > _windowSize)
            {
                Sum -= _volumeWindow.Dequeue();
            }

            if (_CumulativeWindow.Count > _CumulativeSize)
            {
                CumulativeSum -= _CumulativeWindow.Dequeue();
            }
        }

        public double Average => _volumeWindow.Count == 0 ? 0 : Sum / 15;

        public bool IsVolumeExpansion(double multiplier = 1.25)
        {
            if (_volumeWindow.Count < _windowSize)
                return false;

            return CumulativeSum > Average * multiplier;
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
    }
}