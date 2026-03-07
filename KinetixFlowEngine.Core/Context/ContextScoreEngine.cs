namespace KinetixFlowEngine.Core.Context
{
    public class ContextScoreEngine
    {
        public double AdjustScore(
            double score,
            double vwapDev,
            double er,
            double oiChange)
        {
            double erMultiplier = 0.5 + er;

            double oiMultiplier = 1.0;
            if (oiChange > 0)
                oiMultiplier = 1.1;

            double vwapMultiplier = 1.0;

            if (Math.Abs(vwapDev) < 0.002)
                vwapMultiplier = 1.1;

            var adjusted =
                score *
                erMultiplier *
                oiMultiplier *
                vwapMultiplier;

            return Math.Clamp(adjusted, -100, 100);
        }
    }
}