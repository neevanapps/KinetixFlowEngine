using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trend;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class FlowMomentumStrategy : IKinetixStrategy
    {
        public string Name => "FlowMomentum";
        private readonly StrategyConfig _config;

        public FlowMomentumStrategy(StrategyConfigLoader loader)
        {
            _config = loader.Get(Name);
        }

        public StrategySignal Evaluate(KinetixEngineResult r)
        {
            if (r.ScoreZ > _config.MinConfidence && r.VelocityZ > 0.8 && r.ScoreTrend == FlowTrend.Bullish)
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

            if (r.ScoreZ < -1.5 &&
                r.VelocityZ < -0.8 &&
                r.ScoreTrend == FlowTrend.Bearish)
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

            return new StrategySignal
            {
                StrategyName = Name,
                Direction = SignalDirection.None
            };
        }
    }
}