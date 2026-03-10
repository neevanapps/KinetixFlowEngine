using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class FlowProbabilityStrategy : IKinetixStrategy
    {
        public string Name => "FlowProbability";

        private readonly StrategyConfig _config;
        private readonly ILogger<FlowProbabilityStrategy> _logger;

        public FlowProbabilityStrategy(StrategyConfigLoader loader, ILogger<FlowProbabilityStrategy> logger)
        {
            _config = loader.Get(Name);
            _logger = logger;
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            _logger.LogInformation($"PROB STRAT | Fast {r.ProbFastEma:F3} Medium {r.ProbMediumEma:F3} Slow {r.ProbSlowEma:F3}");
            if (r.ProbFastEma > 0.5 && r.ProbMediumEma > 0.5 && r.ProbSlowEma > 0.5)
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

            if (r.ProbFastEma < 0.5 && r.ProbMediumEma < 0.5 && r.ProbSlowEma < 0.5)
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
                bool trendBroken = r.ProbFastEma < 0.5 && r.ProbMediumEma < 0.5 && r.ProbSlowEma < 0.5;

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
                bool trendBroken = r.ProbFastEma > 0.5 && r.ProbMediumEma > 0.5 && r.ProbSlowEma > 0.5;

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