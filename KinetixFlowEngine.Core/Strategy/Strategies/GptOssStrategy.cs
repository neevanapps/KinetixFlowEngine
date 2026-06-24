using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    internal class GptOssStrategy : IKinetixStrategy
    {
        private readonly LlmReviewMemory _memory;
        private readonly StrategyConfig _config;
        private readonly ILogger<GptOssStrategy> _logger;

        public string Name => "GPTOSS";

        public GptOssStrategy(LlmReviewMemory memory, StrategyConfigLoader config, ILogger<GptOssStrategy> logger)
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

            var gptOssReview = reviews.Where(r => r.ModelName.Contains("gpt"))?.FirstOrDefault();
            if (gptOssReview == null)
            {
                _logger.LogInformation("GPTOSS review not found.");
            }
            if (gptOssReview != null && gptOssReview.Assessment.LongConfidence > 50 && gptOssReview.Assessment.Score > 10 && (gptOssReview.Assessment.DirectionalBias == "Long" || gptOssReview.Assessment.RecommendedAction == "Long"))
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = gptOssReview.Assessment.LongConfidence,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            if (gptOssReview != null && gptOssReview.Assessment.ShortConfidence > 50 && gptOssReview.Assessment.Score < -10 && (gptOssReview.Assessment.DirectionalBias == "Short" || gptOssReview.Assessment.RecommendedAction == "Short"))
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = gptOssReview.Assessment.ShortConfidence,
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

            var gptOssReview = reviews.Where(r => r.ModelName.Contains("gpt"))?.FirstOrDefault();

            if (trade.Direction == SignalDirection.Long)
            {
                if (gptOssReview != null && gptOssReview.Assessment.ShortConfidence > 50 && gptOssReview.Assessment.Score < -10 && (gptOssReview.Assessment.DirectionalBias == "Short" || gptOssReview.Assessment.RecommendedAction == "Short"))
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
                if (gptOssReview != null && gptOssReview.Assessment.LongConfidence > 50 && gptOssReview.Assessment.Score > 10 && (gptOssReview.Assessment.DirectionalBias == "Long" || gptOssReview.Assessment.RecommendedAction == "Long"))
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
