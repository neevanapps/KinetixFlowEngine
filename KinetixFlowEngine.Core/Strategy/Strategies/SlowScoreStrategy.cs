using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class SlowScoreStrategy : IKinetixStrategy
    {
        public string Name => "SlowScore";
        private readonly StrategyConfig _config;
        public SlowScoreStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            if ((decimal)r.ScoreSlowEma > r.EmaStability.ScoreSlowEmaLevel1 && r.EmaStability.ScoreSlowEmaLevel1 > r.EmaStability.ScoreSlowEmaLevel2
                        && r.EmaStability.ScoreSlowEmaLevel2 > r.EmaStability.ScoreSlowEmaLevel3)
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

            if ((decimal)r.ScoreSlowEma < r.EmaStability.ScoreSlowEmaLevel1 && r.EmaStability.ScoreSlowEmaLevel1 < r.EmaStability.ScoreSlowEmaLevel2
                        && r.EmaStability.ScoreSlowEmaLevel2 < r.EmaStability.ScoreSlowEmaLevel3)
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
                if ((decimal)r.ScoreSlowEma < r.EmaStability.ScoreSlowEmaLevel1 && r.EmaStability.ScoreSlowEmaLevel1 < r.EmaStability.ScoreSlowEmaLevel2
                        && r.EmaStability.ScoreSlowEmaLevel2 < r.EmaStability.ScoreSlowEmaLevel3)
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
                if ((decimal)r.ScoreSlowEma > r.EmaStability.ScoreSlowEmaLevel1 && r.EmaStability.ScoreSlowEmaLevel1 > r.EmaStability.ScoreSlowEmaLevel2
                        && r.EmaStability.ScoreSlowEmaLevel2 > r.EmaStability.ScoreSlowEmaLevel3)
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
