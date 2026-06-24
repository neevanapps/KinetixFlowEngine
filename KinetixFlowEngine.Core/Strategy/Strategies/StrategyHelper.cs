using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public sealed class StrategyHelper
    {
        private readonly ModelReviewProvider _reviewProvider;
        private readonly ILogger<StrategyHelper> _logger;

        public StrategyHelper(ModelReviewProvider reviewProvider, ILogger<StrategyHelper> logger)
        {
            _reviewProvider = reviewProvider;
            _logger = logger;
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult result, string modelName, StrategyConfig _config)
        {
            var review = _reviewProvider.GetModelReview(modelName);

            if (review == null)
                return NoSignal(_config.StrategyName);

            if (review == null)
            {
                _logger.LogInformation($"{modelName} review not found.");
            }

            if (review != null && review.LongConfidence > 60 && review.Score > 10 && review.ScoreVelocity > 0)
            {
                return new StrategySignal
                {
                    StrategyName = _config.StrategyName,
                    Direction = SignalDirection.Long,
                    Confidence = review.LongConfidence,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            if (review != null && review.ShortConfidence > 60 && review.Score < -10 && review.ScoreVelocity < 0)
            {
                return new StrategySignal
                {
                    StrategyName = _config.StrategyName,
                    Direction = SignalDirection.Short,
                    Confidence = review.ShortConfidence,
                    EnterOnlyAtFairPrice = _config.EnterOnlyAtFairPrice,
                    NotifyThroughTelegram = _config.NotifyThroughTelegram,
                    IsVolumeBased = _config.VolumeBased,
                    TargetAccountIds = _config.AccountIds
                };
            }

            return NoSignal(_config.StrategyName);
        }

        public StrategySignal EvaluateExit(KinetixEngineResult result, ActiveTrade trade, string modelName, StrategyConfig _config)
        {
            var review = _reviewProvider.GetModelReview(modelName); ;

            if (trade.Direction == SignalDirection.Long)
            {
                if (review != null && review.ShortConfidence > 60 && review.Score < -10 && review.ScoreVelocity < 0)
                {
                    return new StrategySignal
                    {
                        StrategyName = _config.StrategyName,
                        ExitSignal = true
                    };
                }

            }
            else if (trade.Direction == SignalDirection.Short)
            {
                if (review != null && review.LongConfidence > 60 && review.Score > 10 && review.ScoreVelocity > 0)
                {
                    return new StrategySignal
                    {
                        StrategyName = _config.StrategyName,
                        ExitSignal = true
                    };
                }
            }
            else
            {
                return NoSignal(_config.StrategyName);
            }
            return NoSignal(_config.StrategyName);
        }

        private StrategySignal NoSignal(string strategyName)
        {
            return new()
            {
                StrategyName = strategyName,
                Direction = SignalDirection.None
            };
        }
    }
}
