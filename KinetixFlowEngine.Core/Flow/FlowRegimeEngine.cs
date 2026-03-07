namespace KinetixFlowEngine.Core.Flow
{
    public class FlowRegimeEngine
    {
        public FlowRegimeSnapshot Detect(
            FlowFeatureSnapshot f,
            double score)
        {
            FlowRegime regime;

            if (Math.Abs(score) > 60 && Math.Abs(f.DeltaVelocity) < 0.05)
            {
                regime = FlowRegime.Trend;
            }
            else if (Math.Abs(f.DeltaVelocity) > 0.25)
            {
                regime = FlowRegime.Transition;
            }
            else if (Math.Abs(score) < 15)
            {
                regime = FlowRegime.Chop;
            }
            else if (Math.Abs(score) > 60 && f.Momentum < 0)
            {
                regime = FlowRegime.Exhaustion;
            }
            else
            {
                regime = FlowRegime.Chop;
            }

            return new FlowRegimeSnapshot
            {
                Regime = regime,
                Score = score
            };
        }
    }
}