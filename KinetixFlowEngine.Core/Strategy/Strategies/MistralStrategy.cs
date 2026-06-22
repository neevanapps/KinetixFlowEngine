using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    internal class MistralStrategy : IKinetixStrategy
    {
        private readonly LlmReviewMemory _memory;
        private readonly StrategyConfig _config;
        private readonly ILogger<MistralStrategy> _logger;

        public string Name => "Mistral";

        public MistralStrategy(LlmReviewMemory memory, StrategyConfigLoader config, ILogger<MistralStrategy> logger)
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

            var mistralReview = reviews.Where(r => r.ModelName.Contains("mistral"))?.FirstOrDefault();

            if (mistralReview == null)
            {
                _logger.LogInformation("Mistral review not found.");
            }

            if (mistralReview != null && mistralReview.Assessment.LongConfidence > 50 && mistralReview.Assessment.Score > 10 && (mistralReview.Assessment.DirectionalBias == "Long" || mistralReview.Assessment.RecommendedAction == "Long"))
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Long,
                    Confidence = mistralReview.Assessment.LongConfidence,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            if (mistralReview != null && mistralReview.Assessment.ShortConfidence > 50 && mistralReview.Assessment.Score < -10 && (mistralReview.Assessment.DirectionalBias == "Short" || mistralReview.Assessment.RecommendedAction == "Short"))
            {
                return new StrategySignal
                {
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = mistralReview.Assessment.ShortConfidence,
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

            var mistralReview = reviews.Where(r => r.ModelName.Contains("mistral"))?.FirstOrDefault();

            if (trade.Direction == SignalDirection.Long)
            {
                if (mistralReview != null && mistralReview.Assessment.ShortConfidence > 50 && mistralReview.Assessment.Score < -10 && (mistralReview.Assessment.DirectionalBias == "Short" || mistralReview.Assessment.RecommendedAction == "Short"))
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
                if (mistralReview != null && mistralReview.Assessment.LongConfidence > 50 && mistralReview.Assessment.Score > 10 && (mistralReview.Assessment.DirectionalBias == "Long" || mistralReview.Assessment.RecommendedAction == "Long"))
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
