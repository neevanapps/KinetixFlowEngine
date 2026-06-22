using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    internal class QwenStrategy : IKinetixStrategy
    {
        private readonly LlmReviewMemory _memory;
        private readonly StrategyConfig _config;
        private readonly ILogger<QwenStrategy> _logger;

        public string Name => "Qwen";

        public QwenStrategy(LlmReviewMemory memory, StrategyConfigLoader config, ILogger<QwenStrategy> logger)
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

            var qwenReview = reviews.Where(r => r.ModelName.Contains("qwen"))?.FirstOrDefault();
            if (qwenReview == null)
            {
                _logger.LogInformation("Qwen review not found.");
            }
            if (qwenReview != null && qwenReview.Assessment.LongConfidence > 50 && qwenReview.Assessment.Score > 10 && (qwenReview.Assessment.DirectionalBias == "Long" || qwenReview.Assessment.RecommendedAction == "Long"))
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = qwenReview.Assessment.LongConfidence,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            if (qwenReview != null && qwenReview.Assessment.ShortConfidence > 50 && qwenReview.Assessment.Score < -10 && (qwenReview.Assessment.DirectionalBias == "Short" || qwenReview.Assessment.RecommendedAction == "Short"))
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = qwenReview.Assessment.ShortConfidence,
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

            var qwenReview = reviews.Where(r => r.ModelName.Contains("qwen3"))?.FirstOrDefault();

            if (trade.Direction == SignalDirection.Long)
            {
                if (qwenReview != null && qwenReview.Assessment.ShortConfidence > 50 && qwenReview.Assessment.Score < -10 && (qwenReview.Assessment.DirectionalBias == "Short" || qwenReview.Assessment.RecommendedAction == "Short"))
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
                if (qwenReview != null && qwenReview.Assessment.LongConfidence > 50 && qwenReview.Assessment.Score > 10 && (qwenReview.Assessment.DirectionalBias == "Long" || qwenReview.Assessment.RecommendedAction == "Long"))
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
