namespace KinetixFlowEngine.Core.Flow.Features
{
    public static class FlowImbalance
    {
        public static double Calculate(decimal buyVolume, decimal sellVolume)
        {
            var total = buyVolume + sellVolume;

            if (total == 0)
                return 0;

            return (double)((buyVolume - sellVolume) / total);
        }
    }
}