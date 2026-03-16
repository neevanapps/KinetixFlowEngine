using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Flow
{
    public class FlowPersistenceSnapshot
    {
        public int BullishDuration { get; set; }
        public int BearishDuration { get; set; }

        public bool StrongBullish => BullishDuration >= 6;
        public bool StrongBearish => BearishDuration >= 6;
    }

    public class FlowPersistenceEngine
    {
        private int _bullishCount = 0;
        private int _bearishCount = 0;

        public FlowPersistenceSnapshot Update(double scoreZ)
        {
            const double threshold = 0.5;

            if (scoreZ > threshold)
            {
                _bullishCount++;
                _bearishCount = Math.Max(0, _bearishCount - 1);
            }
            else if (scoreZ < -threshold)
            {
                _bearishCount++;
                _bullishCount = Math.Max(0, _bullishCount - 1);
            }
            else
            {
                _bullishCount = Math.Max(0, _bullishCount - 1);
                _bearishCount = Math.Max(0, _bearishCount - 1);
            }

            return new FlowPersistenceSnapshot
            {
                BullishDuration = _bullishCount,
                BearishDuration = _bearishCount
            };
        }
    }
}