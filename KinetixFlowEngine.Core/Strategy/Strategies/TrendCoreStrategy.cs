using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class TrendCoreStrategy : IKinetixStrategy
    {
        public string Name => "TrendCore";

        private readonly StrategyConfig _config;

        public TrendCoreStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            bool bullishStructure =
                r.ScoreFastEma > r.ScoreMediumEma &&
                r.ScoreMediumEma > r.ScoreSlowEma;

            bool bearishStructure =
                r.ScoreFastEma < r.ScoreMediumEma &&
                r.ScoreMediumEma < r.ScoreSlowEma;

            bool strongScore = Math.Abs(r.ScoreMediumEma) > 3.0;

            bool probConfirmLong = r.ProbMediumEma > _config.MinConfidence;
            bool probConfirmShort = r.ProbMediumEma < (1 - _config.MinConfidence);

            bool momentumGate =
                Math.Abs(r.VelocityEma) > 0.6 &&
                ((r.ScoreMediumEma > 0 && r.VelocityEma > 0) ||
                 (r.ScoreMediumEma < 0 && r.VelocityEma < 0));

            if (bullishStructure && strongScore && probConfirmLong && momentumGate)
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

            if (bearishStructure && strongScore && probConfirmShort && momentumGate)
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
            bool reversal =
                (trade.Direction == SignalDirection.Long && r.ScoreMediumEma < 0) ||
                (trade.Direction == SignalDirection.Short && r.ScoreMediumEma > 0);

            return new StrategySignal
            {
                StrategyName = Name,
                ExitSignal = reversal
            };
        }
    }
}