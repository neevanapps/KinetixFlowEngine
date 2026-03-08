using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Utils;

namespace KinetixFlowEngine.Core.Strategy
{
    public class StrategyEngine
    {
        private readonly List<IKinetixStrategy> _strategies;
        private readonly FairPriceEngine _fairPriceEngine;

        public StrategyEngine(IEnumerable<IKinetixStrategy> strategies, FairPriceEngine fairPriceEngine)
        {
            _strategies = strategies.ToList();
            _fairPriceEngine = fairPriceEngine;
        }

        public List<StrategySignal> Evaluate(KinetixEngineResult result)
        {
            var signals = new List<StrategySignal>();

            foreach (var strategy in _strategies)
            {
                var signal = strategy.EvaluateEntry(result);

                if (signal.Direction != SignalDirection.None)
                {
                    if (signal.EnterOnlyAtFairPrice)
                    {
                        var price = (decimal)result.Price;

                        bool approved =
                            signal.Direction == SignalDirection.Long
                                ? _fairPriceEngine.IsFairLongEntry(price, result.VWAP, result.ATR)
                                : _fairPriceEngine.IsFairShortEntry(price, result.VWAP, result.ATR);

                        signal.FairPriceApproved = approved;

                        if (!approved)
                            continue;
                    }

                    signals.Add(signal);
                }
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
    }
}