using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    internal class GlmStrategy : IKinetixStrategy
    {
        private readonly LlmReviewMemory _memory;
        private readonly StrategyConfig _config;
        private readonly ILogger<GlmStrategy> _logger;

        public string Name => "GLM";

        public GlmStrategy(LlmReviewMemory memory, StrategyConfigLoader config, ILogger<GlmStrategy> logger)
        {
            _memory = memory;
            _config = config.Get(Name);
            _logger = logger;
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult result)
        {
            var reviews = _memory.GetLatest();

            if (reviews.Count < 2)
                return NoSignal();

            var glmReview = reviews.Where(r => r.ModelName.Contains("glm"))?.FirstOrDefault();
            if (glmReview == null)
            {
                _logger.LogInformation("GLM review not found.");
            }
            if (glmReview != null && glmReview.Assessment.LongConfidence > 50 && glmReview.Assessment.Score > 10 && (glmReview.Assessment.DirectionalBias == "Long" || glmReview.Assessment.RecommendedAction == "Long"))
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = glmReview.Assessment.LongConfidence,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            if (glmReview != null && glmReview.Assessment.ShortConfidence > 50 && glmReview.Assessment.Score < -10 && (glmReview.Assessment.DirectionalBias == "Short" || glmReview.Assessment.RecommendedAction == "Short"))
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = glmReview.Assessment.ShortConfidence,
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

            var glmReview = reviews.Where(r => r.ModelName.Contains("glm"))?.FirstOrDefault();

            if (trade.Direction == SignalDirection.Long)
            {
                if (glmReview != null && glmReview.Assessment.ShortConfidence > 50 && glmReview.Assessment.Score < -10 && (glmReview.Assessment.DirectionalBias == "Short" || glmReview.Assessment.RecommendedAction == "Short"))
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
                if (glmReview != null && glmReview.Assessment.LongConfidence > 50 && glmReview.Assessment.Score > 10 && (glmReview.Assessment.DirectionalBias == "Long" || glmReview.Assessment.RecommendedAction == "Long"))
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

        private StrategySignal NoSignal()
        {
            return new()
            {
                StrategyName = Name,
                Direction = SignalDirection.None
            };
        }
    }
}
