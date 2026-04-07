using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Strategy
{
    public class StrategyEngine
    {
        private readonly List<IKinetixStrategy> _strategies;
        private readonly FairPriceEngine _fairPriceEngine;
        private readonly VolumeEngine _volumeEngine;
        private readonly StrategyConfigLoader _strategyConfigLoader;
        public StrategyEngine(IEnumerable<IKinetixStrategy> strategies, FairPriceEngine fairPriceEngine, VolumeEngine volumeEngine, StrategyConfigLoader strategyConfigLoader)
        {
            _strategies = strategies.ToList();
            _fairPriceEngine = fairPriceEngine;
            _volumeEngine = volumeEngine;
            _strategyConfigLoader = strategyConfigLoader;
        }

        public List<StrategySignal> Evaluate(KinetixEngineResult result)
        {
            var signals = new List<StrategySignal>();

            foreach (var strategy in _strategies)
            {
                var signal = strategy.EvaluateEntry(result);

                if (signal.Direction == SignalDirection.None)
                    continue;

                // Volume check
                if (signal.IsVolumeBased && !_volumeEngine.IsVolumeExpansion())
                    continue;

                // Fair price check
                if (signal.EnterOnlyAtFairPrice)
                {
                    var price = (decimal)result.Price;
                    bool approved = signal.Direction == SignalDirection.Long
                        ? _fairPriceEngine.IsFairLongEntry(price, result.VWAP, result.ATR)
                        : _fairPriceEngine.IsFairShortEntry(price, result.VWAP, result.ATR);

                    signal.FairPriceApproved = approved;
                    if (!approved) continue;
                }

                // NEW: Attach target accounts from config
                var config = _strategyConfigLoader.Get(strategy.Name);
                signal.TargetAccountIds = config.AccountIds ?? new List<string>();

                if (!IsTrendAligned(result, signal.Direction))
                    continue;

                signals.Add(signal);
            }

            return signals;
        }

        // EXIT EVALUATION
        public StrategySignal? EvaluateExit(KinetixEngineResult result, ActiveTrade trade)
        {
            foreach (var strategy in _strategies)
            {
                if (strategy.Name != trade.StrategyName)
                    continue;

                var exit = strategy.EvaluateExit(result, trade);

                if (exit.ExitSignal)
                    return exit;
            }

            return null;
        }

        public bool IsTrendAligned(KinetixEngineResult result, SignalDirection direction)
        {
            double fast = result.ScoreFastEma;
            double medium = result.ScoreMediumEma;
            double slow = result.ScoreSlowEma;
            double pm = result.ProbMediumEma;
            double spread = fast - slow;

            //bool probBull = pm > 0.55;
            //bool probBear = pm < 0.45;
            bool bullish = (fast > medium && medium > slow && spread > 0.4) || (fast > 0 && medium > 0 && slow > 0);
            bool bearish = (fast < medium && medium < slow && -spread > 0.4) || (fast < 0 && medium < 0 && slow < 0);

            return (direction == SignalDirection.Long && bullish)
                || (direction == SignalDirection.Short && bearish);
        }
    }
}