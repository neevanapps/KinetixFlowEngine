namespace KinetixFlowEngine.Core.Signal
{
    public class SignalStabilityEngine
    {
        private int _longCount = 0;
        private int _shortCount = 0;

        private readonly int _requiredSeconds;

        public SignalStabilityEngine(int requiredSeconds = 5)
        {
            _requiredSeconds = requiredSeconds;
        }

        public SignalStabilitySnapshot Update(bool longSignal, bool shortSignal)
        {
            if (longSignal)
                _longCount++;
            else
                _longCount = 0;

            if (shortSignal)
                _shortCount++;
            else
                _shortCount = 0;

            return new SignalStabilitySnapshot
            {
                LongPersistence = _longCount,
                ShortPersistence = _shortCount,
                LongStable = _longCount >= _requiredSeconds,
                ShortStable = _shortCount >= _requiredSeconds
            };
        }
    }
}