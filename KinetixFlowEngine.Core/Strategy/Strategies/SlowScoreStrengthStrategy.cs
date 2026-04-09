using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Strategy.Strategies
{
    public class SlowScoreStrengthStrategy : IKinetixStrategy
    {
        public string Name => "SlowScoreStrength";

        private readonly StrategyConfig _config;
        private readonly SignalStrengthEngine _strength;

        public SlowScoreStrengthStrategy(
            StrategyConfigLoader loader,
            SignalStrengthEngine strength)
        {
            _config = loader.Get(Name);
            _strength = strength;
        }

        public StrategySignal EvaluateEntry(KinetixEngineResult r)
        {
            decimal l1 = r.EmaStability.ScoreSlowEmaLevel1;
            decimal l2 = r.EmaStability.ScoreSlowEmaLevel2;
            decimal l3 = r.EmaStability.ScoreSlowEmaLevel3;

            decimal spread = l1 - l3;

            bool bullish = l1 > l2 && l2 > l3 && spread > 0.5m && l1 > 0.2m;
            bool bearish = l1 < l2 && l2 < l3 && -spread > 0.5m && l1 < -0.2m;

            // =========================
            // NEW: Strength filter
            // =========================
            double strength = _strength.LastStrength;
            double delta = _strength.Delta;

            const double minDelta = 0.03;

            bool allowLong = strength > 0 && delta > minDelta;
            bool allowShort = strength < 0 && delta < -minDelta;

            if (bullish && allowLong)
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

            if (bearish && allowShort)
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

            return new StrategySignal { StrategyName = Name, Direction = SignalDirection.None };
        }

        public StrategySignal EvaluateExit(KinetixEngineResult r, ActiveTrade trade)
        {
            // reuse original exit logic (NO change)
            decimal l1 = r.EmaStability.ScoreSlowEmaLevel1;
            decimal l2 = r.EmaStability.ScoreSlowEmaLevel2;
            decimal l3 = r.EmaStability.ScoreSlowEmaLevel3;

            decimal spread = l1 - l3;

            bool bullish = l1 > l2 && l2 > l3 && spread > 0.5m && l1 > 0.2m;
            bool bearish = l1 < l2 && l2 < l3 && -spread > 0.5m && l1 < -0.2m;

            if (trade.Direction == SignalDirection.Long && bearish)
                return new StrategySignal { StrategyName = Name, ExitSignal = true };

            if (trade.Direction == SignalDirection.Short && bullish)
                return new StrategySignal { StrategyName = Name, ExitSignal = true };

            return new StrategySignal { ExitSignal = false };
        }
    }
}