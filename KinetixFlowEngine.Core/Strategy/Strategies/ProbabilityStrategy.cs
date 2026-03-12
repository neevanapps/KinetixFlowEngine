using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class ProbabilityStrategy : IKinetixStrategy
    {
        public string Name => "ProbabilityStrategy";

        private readonly StrategyConfig _config;
        private readonly ILogger<ProbabilityStrategy> _logger;

        public ProbabilityStrategy(StrategyConfigLoader loader, ILogger<ProbabilityStrategy> logger)
        {
            _config = loader.Get(Name);
            _logger = logger;
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            if (r.ProbFastEma > 0.55 && r.ProbMediumEma > 0.55 && r.ProbSlowEma > 0.55)
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

            if (r.ProbFastEma < 0.45 && r.ProbMediumEma < 0.45 && r.ProbSlowEma < 0.45)
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
                bool trendBroken = r.ProbFastEma < 0.45 && r.ProbMediumEma < 0.45 && r.ProbSlowEma < 0.45;

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
                bool trendBroken = r.ProbFastEma > 0.55 && r.ProbMediumEma > 0.55 && r.ProbSlowEma > 0.55;

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