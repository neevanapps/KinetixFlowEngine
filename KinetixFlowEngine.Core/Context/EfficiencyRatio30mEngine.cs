namespace KinetixFlowEngine.Core.Context
{
    public class EfficiencyRatio30mEngine : EfficiencyRatioEngine
    {
        public EfficiencyRatio30mEngine() : base(360) { }
        // 360 samples × 5s = 30 minutes
    }
}