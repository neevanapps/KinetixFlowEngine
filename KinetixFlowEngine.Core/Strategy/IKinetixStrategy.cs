using KinetixFlowEngine.Core.Engine;

namespace KinetixFlowEngine.Core.Strategy
{
    public interface IKinetixStrategy
    {
        string Name { get; }

        StrategySignal Evaluate(KinetixEngineResult result);
    }
}