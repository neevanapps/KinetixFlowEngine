using System.Collections.Generic;

namespace KinetixFlowEngine.Core.Context
{
    public class VolumeEngine
    {
        private readonly Queue<double> _volumeWindow = new();

        private readonly int _windowSize;

        public double Sum;

        public VolumeEngine(int windowSize = 60)
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

        public double Average => _volumeWindow.Count == 0 ? 0 : Sum / _volumeWindow.Count;

        public bool IsVolumeExpansion(double currentVolume, double multiplier = 1.3)
        {
            if (_volumeWindow.Count < _windowSize)
                return true;

            return currentVolume > Average * multiplier;
        }
    }
}