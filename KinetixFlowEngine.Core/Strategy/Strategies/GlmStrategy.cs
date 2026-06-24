using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Trading;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    internal class GlmStrategy : IKinetixStrategy
    {
        private readonly StrategyHelper _helper;
        private readonly StrategyConfig _config;

        public string Name => "GLM";
        public string ModelName => "zai-glm-4.7";

        public GlmStrategy(StrategyHelper helper, StrategyConfigLoader config)
        {
            _helper = helper;
            _config = config.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult result)
        {
            return _helper.EvaluateEntry(result, ModelName, _config);
        }

        public StrategySignal EvaluateExit(KinetixEngineResult result, ActiveTrade trade)
        {
            return _helper.EvaluateExit(result, trade, ModelName, _config);
        }
    }
}
