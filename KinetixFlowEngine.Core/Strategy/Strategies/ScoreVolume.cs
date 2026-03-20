using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class ScoreVolume : IKinetixStrategy
    {
        public string Name => "ScoreVolume";
        private readonly StrategyConfig _config;
        double threshold = 2;
        public ScoreVolume(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            if (r.ScoreFastEma > threshold + 2 && r.ScoreMediumEma > threshold + 1 && r.ScoreSlowEma > threshold)
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

            if (r.ScoreFastEma < -threshold - 2 && r.ScoreMediumEma < -threshold - 1 && r.ScoreSlowEma < -threshold)
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
                bool trendBroken = r.ScoreFastEma < -threshold - 2 && r.ScoreMediumEma < -threshold - 1 && r.ScoreSlowEma < -threshold;

                if (trendBroken)
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
                bool trendBroken = r.ScoreFastEma > threshold + 2 && r.ScoreMediumEma > threshold + 1 && r.ScoreSlowEma > threshold;

                if (trendBroken)
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
