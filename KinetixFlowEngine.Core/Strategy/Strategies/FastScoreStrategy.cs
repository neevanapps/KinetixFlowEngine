using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class FastScoreStrategy : IKinetixStrategy
    {
        public string Name => "FastScore";
        private readonly StrategyConfig _config;
        public FastScoreStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            decimal slow = (decimal)r.ScoreFastEma;
            decimal l1 = r.EmaStability.ScoreFastEmaLevel1;
            decimal l2 = r.EmaStability.ScoreFastEmaLevel2;
            decimal l3 = r.EmaStability.ScoreFastEmaLevel3;

            bool bullishStructure = slow > l1 && l1 > l3;
            bool bearishStructure = slow < l1 && l1 < l3;

            bool strongBull = l1 > 1.0m;
            bool strongBear = l1 < -1.0m;

            bool spreadValid = Math.Abs(l1 - l3) > 0.4m;

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
            decimal slow = (decimal)r.ScoreFastEma;
            decimal l1 = r.EmaStability.ScoreFastEmaLevel1;
            decimal l2 = r.EmaStability.ScoreFastEmaLevel2;
            decimal l3 = r.EmaStability.ScoreFastEmaLevel3;

            bool bullishStructure = slow > l1 && l1 > l3;
            bool bearishStructure = slow < l1 && l1 < l3;

            bool strongBull = l1 > 1.0m;
            bool strongBear = l1 < -1.0m;

            bool spreadValid = Math.Abs(l1 - l3) > 0.4m;

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

