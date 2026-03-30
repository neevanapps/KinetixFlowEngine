using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class FastProbStrategy : IKinetixStrategy
    {
        public string Name => "FastProb";
        private readonly StrategyConfig _config;
        public FastProbStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            decimal slow = (decimal)r.ProbFastEma;
            decimal l1 = r.EmaStability.ProbFastEmaLevel1;
            decimal l2 = r.EmaStability.ProbFastEmaLevel2;
            decimal l3 = r.EmaStability.ProbFastEmaLevel3;

            decimal divergence = l2 - l3;
            bool bullish = l2 > 0.52m && l2 > l3 && divergence > 0.01m;
            bool bearish = l2 < 0.48m && l2 < l3 && divergence < -0.01m;

            if (bullish)
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

            if (bearish)
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
            decimal slow = (decimal)r.ProbFastEma;
            decimal l1 = r.EmaStability.ProbFastEmaLevel1;
            decimal l2 = r.EmaStability.ProbFastEmaLevel2;
            decimal l3 = r.EmaStability.ProbFastEmaLevel3;

            decimal divergence = l2 - l3;
            bool bullish = l2 > 0.52m && l2 > l3 && divergence > 0.01m;
            bool bearish = l2 < 0.48m && l2 < l3 && divergence < -0.01m;

            if (trade.Direction == SignalDirection.Long)
            {
                if (bearish)
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
                if (bullish)
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
