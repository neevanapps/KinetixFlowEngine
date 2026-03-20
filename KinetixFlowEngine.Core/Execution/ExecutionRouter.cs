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

            //Simulation should work as its whether account is mapped ot not. It can help test the strategy logic without worrying about account mapping.
            _simPipeline.Execute(signal, price, atr, context);


            // ✅ PROP routing
            foreach (var accId in cfg.AccountIds)
            {
                _accountPipeline.Execute(accId, signal, price, atr, context);
            }
        }
    }
}