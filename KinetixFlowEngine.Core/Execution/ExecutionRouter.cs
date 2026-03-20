using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Execution
{
    public interface IExecutionRouter
    {
        void Route(StrategySignal signal, decimal price, decimal atr, KinetixEngineResult context);
    }

    public class ExecutionRouter : IExecutionRouter
    {
        private readonly StrategyConfigLoader _config;
        private readonly IAccountExecutionPipeline _accountPipeline;
        private readonly ISimExecutionPipeline _simPipeline;
        private readonly ILogger<ExecutionRouter> _logger;


        public ExecutionRouter(
            StrategyConfigLoader config,
            IAccountExecutionPipeline accountPipeline,
            ISimExecutionPipeline simPipeline,
            ILogger<ExecutionRouter> logger)
        {
            _config = config;
            _accountPipeline = accountPipeline;
            _simPipeline = simPipeline;
            _logger = logger;
        }

        public void Route(StrategySignal signal, decimal price, decimal atr, KinetixEngineResult context)
        {
            var cfg = _config.Get(signal.StrategyName);

            if (cfg == null || !cfg.Enabled)
            {
                _logger.LogDebug("Dropped: {Strategy} disabled", signal.StrategyName);
                return;
            }

            // ✅ SIM mode
            if (cfg.AccountIds == null || cfg.AccountIds.Count == 0)
            {
                _simPipeline.Execute(signal, price, atr, context);
                return;
            }

            // ✅ PROP routing
            foreach (var accId in cfg.AccountIds)
            {
                _accountPipeline.Execute(accId, signal, price, atr, context);
            }
        }
    }
}