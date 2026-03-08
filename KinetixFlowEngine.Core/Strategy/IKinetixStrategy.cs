using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Strategy
{
    public interface IKinetixStrategy
    {
        string Name { get; }

        StrategySignal EvaluateEntry(KinetixEngineResult result);

        StrategySignal EvaluateExit(KinetixEngineResult result, ActiveTrade trade);

    }
}