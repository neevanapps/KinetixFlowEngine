namespace KinetixFlowEngine.Core.Strategy
{
    public class StrategyAggregator
    {
        public StrategySignal? SelectSignal(List<StrategySignal> signals)
        {
            if (signals == null || signals.Count == 0)
                return null;

            return signals
                .OrderByDescending(x => x.Confidence)
                .First();
        }
    }
}