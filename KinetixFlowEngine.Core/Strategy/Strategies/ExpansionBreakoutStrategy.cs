using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class ExpansionBreakoutStrategy : IKinetixStrategy
    {
        public string Name => "ExpansionBreakout";

        private readonly StrategyConfig _config;

        public ExpansionBreakoutStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            bool bullishShift =
                r.ScoreFastEma > r.ScoreMediumEma &&
                r.ScoreMediumEma > 0;

            bool bearishShift =
                r.ScoreFastEma < r.ScoreMediumEma &&
                r.ScoreMediumEma < 0;

            bool strongVelocity = Math.Abs(r.VelocityEma) > 1.0;

            bool probStrongLong = r.ProbMediumEma > _config.MinConfidence;
            bool probStrongShort = r.ProbMediumEma < (1 - _config.MinConfidence);

            if (bullishShift && strongVelocity && probStrongLong)
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

            if (bearishShift && strongVelocity && probStrongShort)
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
            bool momentumDying =
                (trade.Direction == SignalDirection.Long && r.VelocityEma < 0.2) ||
                (trade.Direction == SignalDirection.Short && r.VelocityEma > -0.2);

            return new StrategySignal
            {
                StrategyName = Name,
                ExitSignal = momentumDying
            };
        }
    }
}