using System.Collections.Generic;
using System.Linq;

namespace KinetixFlowEngine.Core.Utils
{
    public class RollingWindowBuffer
    {
        private readonly Queue<double> _window = new();
        private readonly int _maxSize;

        public RollingWindowBuffer(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Add(double value)
        {
            _window.Enqueue(value);

            if (_window.Count > _maxSize)
                _window.Dequeue();
        }

        public IReadOnlyList<double> GetValues()
        {
            return _window.ToList();
        }

        public RollingWindowState GetState()
        {
            return new RollingWindowState
            {
                Values = _window.ToList()
            };
        }

        public void Restore(RollingWindowState state)
        {
            _window.Clear();

            foreach (var v in state.Values)
                _window.Enqueue(v);
        }

        public int Count => _window.Count;
    }

    public class RollingWindowState
    {
        public List<double> Values { get; set; } = new();
    }
}