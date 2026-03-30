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
            decimal slow = (decimal)r.ScoreMediumEma;
            decimal l1 = r.EmaStability.ScoreMediumEmaLevel1;
            decimal l2 = r.EmaStability.ScoreMediumEmaLevel2;
            decimal l3 = r.EmaStability.ScoreMediumEmaLevel3;

            bool bullishStructure = l2 > l3;
            bool bearishStructure = l2 < l3;

            bool strongBull = l2 > 0.6m;
            bool strongBear = l2 < -0.6m;

            bool spreadValid = Math.Abs(l2 - l3) > 0.5m;

            if (bullishStructure && strongBull && spreadValid)
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

            if (bearishStructure && strongBear && spreadValid)
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

            return new StrategySignal { StrategyName = Name, Direction = SignalDirection.None };
        }

        public StrategySignal EvaluateExit(KinetixEngineResult r, ActiveTrade trade)
        {
            decimal slow = (decimal)r.ScoreMediumEma;
            decimal l1 = r.EmaStability.ScoreMediumEmaLevel1;
            decimal l2 = r.EmaStability.ScoreMediumEmaLevel2;
            decimal l3 = r.EmaStability.ScoreMediumEmaLevel3;

            bool bullishStructure = l2 > l3;
            bool bearishStructure = l2 < l3;

            bool strongBull = l2 > 0.6m;
            bool strongBear = l2 < -0.6m;

            bool spreadValid = Math.Abs(l2 - l3) > 0.5m;

            if (trade.Direction == SignalDirection.Long)
            {
                if (bearishStructure && strongBear && spreadValid)
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
                if (bullishStructure && strongBull && spreadValid)
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
