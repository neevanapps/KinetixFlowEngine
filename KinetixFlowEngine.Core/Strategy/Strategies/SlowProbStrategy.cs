using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class SlowProbStrategy : IKinetixStrategy
    {
        public string Name => "SlowProb";
        private readonly StrategyConfig _config;
        public SlowProbStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            decimal l1 = r.EmaStability.ProbSlowEmaLevel1;
            decimal l2 = r.EmaStability.ProbSlowEmaLevel2;
            decimal l3 = r.EmaStability.ProbSlowEmaLevel3;
            decimal spread = l1 - l3;

            bool bullish = l1 > l2 && l2 > l3 && spread > 0.05m;
            bool bearish = l1 < l2 && l2 < l3 && -spread > 0.05m;

            if (bullish)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = (double)(l2 + 0.5m),
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
                    Confidence = (double)(0.5m + l2),
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
            decimal l1 = r.EmaStability.ProbSlowEmaLevel1;
            decimal l2 = r.EmaStability.ProbSlowEmaLevel2;
            decimal l3 = r.EmaStability.ProbSlowEmaLevel3;
            decimal spread = l1 - l3;

            bool bullish = l1 > l2 && l2 > l3 && spread > 0.05m;
            bool bearish = l1 < l2 && l2 < l3 && -spread > 0.05m;

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
