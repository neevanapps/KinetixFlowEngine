using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    internal class HybridStrategy : IKinetixStrategy
    {
        public string Name => "HybridStrategy";

        private readonly StrategyConfig _config;
        private readonly ILogger<HybridStrategy> _logger;

        public HybridStrategy(StrategyConfigLoader loader, ILogger<HybridStrategy> logger)
        {
            _config = loader.Get(Name);
            _logger = logger;
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            bool scoreAligned = r.ScoreFastEma > 0 && r.ScoreMediumEma > 0 && r.ScoreSlowEma > 0;
            if (r.ProbFastEma > 0.55 && r.ProbMediumEma > 0.55 && r.ProbSlowEma > 0.55 && scoreAligned)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = r.ScoreZ,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram
                };
            }

            scoreAligned = r.ScoreFastEma < 0 && r.ScoreMediumEma < 0 && r.ScoreSlowEma < 0;
            if (r.ProbFastEma < 0.45 && r.ProbMediumEma < 0.45 && r.ProbSlowEma < 0.45 && scoreAligned)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = r.ScoreZ,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram
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
            bool scoreAligned = r.ScoreFastEma < 0 && r.ScoreMediumEma < 0 && r.ScoreSlowEma < 0;
            if (trade.Direction == SignalDirection.Long)
            {
                bool trendBroken = r.ProbFastEma < 0.45 && r.ProbMediumEma < 0.45 && r.ProbSlowEma < 0.45;

                if (trendBroken && scoreAligned)
                {
                    return new StrategySignal
                    {
                        StrategyName = Name,
                        ExitSignal = true
                    };
                }
            }
            scoreAligned = r.ScoreFastEma > 0 && r.ScoreMediumEma > 0 && r.ScoreSlowEma > 0;

            if (trade.Direction == SignalDirection.Short)
            {
                bool trendBroken = r.ProbFastEma > 0.55 && r.ProbMediumEma > 0.55 && r.ProbSlowEma > 0.55;

                if (trendBroken && scoreAligned)
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
