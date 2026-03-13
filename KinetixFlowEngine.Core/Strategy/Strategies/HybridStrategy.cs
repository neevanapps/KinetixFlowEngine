using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    internal class HybridStrategy : IKinetixStrategy
    {
        public string Name => "HybridStrategy";

        private readonly StrategyConfig _config;
        private readonly ILogger<HybridStrategy> _logger;

        public HybridStrategy(StrategyConfigLoader loader, ILogger<HybridStrategy> logger)
        {
            _config = loader.Get(Name);
            _logger = logger;
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            if (r.ProbFastEma > 0.60 && r.ProbMediumEma > 0.56 && r.ProbSlowEma > 0.52)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = r.ScoreZ,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram
                };
            }
            if (r.ProbFastEma < 0.40 && r.ProbMediumEma < 0.44 && r.ProbSlowEma < 0.48)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = r.ScoreZ,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram
                };
            }

            return new StrategySignal
            {
                StrategyName = Name,
                Direction = SignalDirection.None
            };
        }

        public StrategySignal EvaluateExit(KinetixEngineResult r, ActiveTrade trade)
        {
            if (trade.Direction == SignalDirection.Long)
            {
                bool trendBroken = r.ProbFastEma < 0.40 && r.ProbMediumEma < 0.44 && r.ProbSlowEma < 0.48;

                if (trendBroken)
                {
                    return new StrategySignal
                    {
                        StrategyName = Name,
                        ExitSignal = true
                    };
                }
            }
            if (trade.Direction == SignalDirection.Short)
            {
                bool trendBroken = r.ProbFastEma > 0.60 && r.ProbMediumEma > 0.56 && r.ProbSlowEma > 0.52;

                if (trendBroken)
                {
                    return new StrategySignal
                    {
                        StrategyName = Name,
                        ExitSignal = true
                    };
                }
            }

            return new StrategySignal { ExitSignal = false };
        }
    }
}
