namespace KinetixFlowEngine.Core.Flow
{
    public class FlowScoreEngine
    {
        public double CalculateScore(double composite)
        {
            double score = composite * 100;

            if (score > 100)
                score = 100;

            if (score < -100)
                score = -100;

            return score;
        }
    }
}