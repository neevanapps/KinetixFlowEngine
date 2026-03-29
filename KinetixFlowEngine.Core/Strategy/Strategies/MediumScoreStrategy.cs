using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class MediumScoreStrategy : IKinetixStrategy
    {
        public string Name => "MediumScore";
        private readonly StrategyConfig _config;
        public MediumScoreStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            if ((decimal)r.ScoreMediumEma > r.EmaStability.ScoreMediumEmaLevel1 && r.EmaStability.ScoreMediumEmaLevel1 > r.EmaStability.ScoreMediumEmaLevel2
                        && r.EmaStability.ScoreMediumEmaLevel2 > r.EmaStability.ScoreMediumEmaLevel3)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = r.ScoreZ,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            if ((decimal)r.ScoreMediumEma < r.EmaStability.ScoreMediumEmaLevel1 && r.EmaStability.ScoreMediumEmaLevel1 < r.EmaStability.ScoreMediumEmaLevel2
                        && r.EmaStability.ScoreMediumEmaLevel2 < r.EmaStability.ScoreMediumEmaLevel3)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = r.ScoreZ,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
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
                if ((decimal)r.ScoreMediumEma < r.EmaStability.ScoreMediumEmaLevel1 && r.EmaStability.ScoreMediumEmaLevel1 < r.EmaStability.ScoreMediumEmaLevel2
                        && r.EmaStability.ScoreMediumEmaLevel2 < r.EmaStability.ScoreMediumEmaLevel3)
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
                if ((decimal)r.ScoreMediumEma > r.EmaStability.ScoreMediumEmaLevel1 && r.EmaStability.ScoreMediumEmaLevel1 > r.EmaStability.ScoreMediumEmaLevel2
                        && r.EmaStability.ScoreMediumEmaLevel2 > r.EmaStability.ScoreMediumEmaLevel3)
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
