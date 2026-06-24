using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Trading;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    internal class ConsensusStrategy : IKinetixStrategy
    {
        private readonly ConsensusProvider _reviewProvider;
        private readonly StrategyConfig _config;
        private readonly ILogger<ConsensusStrategy> _logger;

        public string Name => "Consensus";

        public ConsensusStrategy(ConsensusProvider reviewProvider, StrategyConfigLoader config, ILogger<ConsensusStrategy> logger)
        {
            _reviewProvider = reviewProvider;
            _config = config.Get(Name);
            _logger = logger;
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult result)
        {
            var review = _reviewProvider.GetConsensus();

            if (review == null)
                return NoSignal();

            if (review == null)
            {
                _logger.LogInformation("Consensus review not found.");
            }

            if (review != null && review.LongConfidence > 60 && review.Score > 10 && review.ScoreVelocity > 0)
            {
                return new StrategySignal
                {
                    StrategyName = Name,
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
                    StrategyName = Name,
                    Direction = SignalDirection.Short,
                    Confidence = review.ShortConfidence,
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
            var review = _reviewProvider.GetConsensus();

            if (trade.Direction == SignalDirection.Long)
            {
                if (review != null && review.ShortConfidence > 60 && review.Score < -10 && review.ScoreVelocity < 0)
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
                if (review != null && review.LongConfidence > 60 && review.Score > 10 && review.ScoreVelocity > 0)
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
