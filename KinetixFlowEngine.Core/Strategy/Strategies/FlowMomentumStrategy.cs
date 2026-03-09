using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class FlowMomentumStrategy : IKinetixStrategy
    {
        public string Name => "FlowMomentum";
        private readonly StrategyConfig _config;

        public FlowMomentumStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            double spread = r.ScoreMediumEma - r.ScoreSlowEma;
            if (r.ScoreFastEma > r.ScoreMediumEma && r.ScoreMediumEma > r.ScoreSlowEma && spread > 2 && r.ScoreMediumEma > 0)
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

            spread = r.ScoreSlowEma - r.ScoreMediumEma;
            if (r.ScoreFastEma < r.ScoreMediumEma && r.ScoreMediumEma < r.ScoreSlowEma && spread > 2 && r.ScoreMediumEma < 0)
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
                bool trendBroken = r.ScoreFastEma < r.ScoreMediumEma && r.ScoreMediumEma < r.ScoreSlowEma && r.ScoreMediumEma < 0;

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
                bool trendBroken = r.ScoreFastEma > r.ScoreMediumEma && r.ScoreMediumEma > r.ScoreSlowEma && r.ScoreMediumEma > 0;

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