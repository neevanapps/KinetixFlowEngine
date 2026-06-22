using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public sealed class LlmConsensusStrategy : IKinetixStrategy
    {
        private readonly LlmReviewMemory _memory;
        private readonly StrategyConfig _config;
        private readonly ILogger<LlmConsensusStrategy> _logger;

        public string Name => "LlmConsensus";

        public LlmConsensusStrategy(LlmReviewMemory memory, StrategyConfigLoader config, ILogger<LlmConsensusStrategy> logger)
        {
            _memory = memory;
            _config = config.Get(Name);
            _logger = logger;
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult result)
        {
            var reviews = _memory.GetLatest();
            _logger.LogInformation("Evaluating Entry | Reviews Count: {Count}", reviews.Count);
            if (reviews.Count < 2)
                return NoSignal();

            int longVotes = 0;
            int shortVotes = 0;

            double confidence = 0;

            foreach (var review in reviews)
            {
                confidence += Math.Max(review.Assessment.LongConfidence, review.Assessment.ShortConfidence);

                switch (review.Assessment.DirectionalBias)
                {
                    case "Long":
                        longVotes++;
                        break;

                    case "Short":
                        shortVotes++;
                        break;
                }

                switch (review.Assessment.RecommendedAction)
                {
                    case "Long":
                        longVotes++;
                        break;

                    case "Short":
                        shortVotes++;
                        break;
                }
            }

            confidence /= reviews.Count;

            if (longVotes >= 2)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = confidence,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            if (shortVotes >= 2)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = confidence,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            return NoSignal();
        }

        public StrategySignal EvaluateExit(KinetixEngineResult result, ActiveTrade trade)
        {
            var reviews = _memory.GetLatest();
            if (reviews.Count < 2)
                return NoSignal();

            int longVotes = 0;
            int shortVotes = 0;

            double confidence = 0;

            foreach (var review in reviews)
            {
                confidence += Math.Max(review.Assessment.LongConfidence, review.Assessment.ShortConfidence);

                switch (review.Assessment.DirectionalBias)
                {
                    case "Long":
                        longVotes++;
                        break;

                    case "Short":
                        shortVotes++;
                        break;
                }
                switch (review.Assessment.RecommendedAction)
                {
                    case "Long":
                        longVotes++;
                        break;

                    case "Short":
                        shortVotes++;
                        break;
                }
            }

            confidence /= reviews.Count;

            if (trade.Direction == SignalDirection.Long)
            {
                if (shortVotes >= 2)
                {
                    return new StrategySignal
                    {
                        StrategyName = Name,
                        ExitSignal = true
                    };
                }

            }
            else if (trade.Direction == SignalDirection.Short)
            {
                if (longVotes >= 2)
                {
                    return new StrategySignal
                    {
                        StrategyName = Name,
                        ExitSignal = true
                    };
                }
            }
            else
            {
                return NoSignal();
            }
            return NoSignal();
        }

        private static StrategySignal NoSignal()
        {
            return new()
            {
                StrategyName = "LlmConsensus",
                Direction = SignalDirection.None
            };
        }
    }
}
