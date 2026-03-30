using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class MediumProbStrategy : IKinetixStrategy
    {
        public string Name => "MediumProb";
        private readonly StrategyConfig _config;
        public MediumProbStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            decimal slow = (decimal)r.ProbMediumEma;
            decimal l1 = r.EmaStability.ProbMediumEmaLevel1;
            decimal l2 = r.EmaStability.ProbMediumEmaLevel2;
            decimal l3 = r.EmaStability.ProbMediumEmaLevel3;

            // =========================
            // 1. DIRECTION (RELAXED)
            // =========================
            bool bullishStructure = slow > l1 && l1 > l3;
            bool bearishStructure = slow < l1 && l1 < l3;

            // =========================
            // 2. STRENGTH FILTER
            // =========================
            bool strongBull = l1 > 0.52m;
            bool strongBear = l1 < 0.48m;

            // =========================
            // 3. TREND SPREAD (IMPORTANT)
            // =========================
            bool spreadValid = Math.Abs(l1 - l3) > 0.015m;

            if (bullishStructure && strongBull && spreadValid)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = (double)slow,
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
                    Confidence = (double)slow,
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
            decimal slow = (decimal)r.ProbMediumEma;
            decimal l1 = r.EmaStability.ProbMediumEmaLevel1;
            decimal l2 = r.EmaStability.ProbMediumEmaLevel2;
            decimal l3 = r.EmaStability.ProbMediumEmaLevel3;

            // =========================
            // 1. DIRECTION (RELAXED)
            // =========================
            bool bullishStructure = slow > l1 && l1 > l3;
            bool bearishStructure = slow < l1 && l1 < l3;

            // =========================
            // 2. STRENGTH FILTER
            // =========================
            bool strongBull = l1 > 0.52m;
            bool strongBear = l1 < 0.48m;

            // =========================
            // 3. TREND SPREAD (IMPORTANT)
            // =========================
            bool spreadValid = Math.Abs(l1 - l3) > 0.015m;

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
