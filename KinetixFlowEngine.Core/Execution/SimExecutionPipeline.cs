using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Execution
{
    public interface ISimExecutionPipeline
    {
        Task Execute(StrategySignal signal, decimal price, decimal atr, KinetixEngineResult context);
    }

    public class SimExecutionPipeline : ISimExecutionPipeline
    {
        private readonly PositionManager _positionManager;

        public SimExecutionPipeline(PositionManager positionManager)
        {
            _positionManager = positionManager;
        }

        public async Task Execute(StrategySignal signal, decimal price, decimal atr, KinetixEngineResult context)
        {
            if (await _positionManager.HasPosition(signal.StrategyName, "SIM"))
                return;

            _positionManager.TryEnterTrade(
                signal,
                price,
                (double)atr,
                context,
                1,              // fixed size for sim
                "SIM",
                Guid.NewGuid().ToString());
        }
    }
}