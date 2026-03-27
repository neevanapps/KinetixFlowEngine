using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class PullbackContinuationStrategy : IKinetixStrategy
    {
        public string Name => "PullbackContinuation";

        private readonly StrategyConfig _config;

        public PullbackContinuationStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            bool bullishStructure =
                r.ScoreMediumEma > r.ScoreSlowEma;

            bool bearishStructure =
                r.ScoreMediumEma < r.ScoreSlowEma;

            // Pullback condition
            bool pullbackLong =
                r.ScoreFastEma < r.ScoreMediumEma &&
                r.ScoreMediumEma > 0;

            bool pullbackShort =
                r.ScoreFastEma > r.ScoreMediumEma &&
                r.ScoreMediumEma < 0;

            // Recovery signal
            bool recoveringLong = r.VelocityEma > 0;
            bool recoveringShort = r.VelocityEma < 0;

            bool probSupportLong = r.ProbMediumEma > _config.MinConfidence;
            bool probSupportShort = r.ProbMediumEma < (1 - _config.MinConfidence);

            if (bullishStructure && pullbackLong && recoveringLong && probSupportLong)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = (double)r.ProbMediumEma,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            if (bearishStructure && pullbackShort && recoveringShort && probSupportShort)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = (double)r.ProbMediumEma,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            return new StrategySignal { StrategyName = Name, Direction = SignalDirection.None };
        }

        public StrategySignal EvaluateExit(KinetixEngineResult r, ActiveTrade trade)
        {
            bool trendLost =
                (trade.Direction == SignalDirection.Long && r.ScoreMediumEma < 0) ||
                (trade.Direction == SignalDirection.Short && r.ScoreMediumEma > 0);

            return new StrategySignal
            {
                StrategyName = Name,
                ExitSignal = trendLost
            };
        }
    }
}