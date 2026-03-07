namespace KinetixFlowEngine.Core.Signal
{
    public class SignalStabilitySnapshot
    {
        public bool LongStable { get; set; }

        public bool ShortStable { get; set; }

        public int LongPersistence { get; set; }

        public int ShortPersistence { get; set; }
    }
}