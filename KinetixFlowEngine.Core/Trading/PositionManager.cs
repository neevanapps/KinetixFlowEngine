using KinetixFlowEngine.Core.Config;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Strategy;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Trading
{
    public class PositionManager
    {
        private readonly TradePersistence _persistence;

        public event Action<ActiveTrade>? Target1Reached;
        public event Action<ActiveTrade, decimal, long>? TradeClosed;
        private readonly Dictionary<string, ActiveTrade> _activeTrades;
        private readonly bool _replayMode;
        private readonly Dictionary<string, long> _lastExitTime = new();
        private readonly Dictionary<string, long> _lastEntryTimestamp = new();
        public PositionManager(TradePersistence persistence, IOptions<FlowEngineOptions> options)
        {
            _persistence = persistence;
            _replayMode = options.Value.ReplayMode;

            if (_replayMode)
                _activeTrades = new Dictionary<string, ActiveTrade>();
            else
                _activeTrades = _persistence.Load();
        }

        public bool HasPosition(string strategy)
        {
            return _activeTrades.ContainsKey(strategy);
        }

        public ActiveTrade? GetPosition(string strategy)
        {
            _activeTrades.TryGetValue(strategy, out var trade);
            return trade;
        }

        public IEnumerable<ActiveTrade> GetAllPositions()
        {
            return _activeTrades.Values;
        }

        public void TryEnterTrade(StrategySignal signal, decimal price, double atr, KinetixEngineResult r, long timestamp)
        {
            if (_activeTrades.ContainsKey(signal.StrategyName))
                return;
            if (_lastEntryTimestamp.TryGetValue(signal.StrategyName, out var lastTs))
            {
                if (lastTs == timestamp)
                    return; // prevent duplicate entry in same timestamp
            }

            var now = _replayMode ? timestamp : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (_lastExitTime.TryGetValue(signal.StrategyName, out var lastExit))
            {
                var cooldown = 60 * 1000;

                if (now - lastExit < cooldown)
                    return;
            }

            decimal atrValue = (decimal)atr;
            decimal stopDistance = Math.Min(price * 0.01m, atrValue * 3);

            var trade = new ActiveTrade
            {
                StrategyName = signal.StrategyName,
                Direction = signal.Direction,
                EntryPrice = price,
                StopLoss = signal.Direction == SignalDirection.Long
                    ? price - stopDistance
                    : price + stopDistance,

                Target1 = signal.Direction == SignalDirection.Long
                    ? price + stopDistance
                    : price - stopDistance,

                TrailingStop = stopDistance,
                InitialSize = 1,
                RemainingSize = 1,

                EntryTimeMs = timestamp,
                NotifyThroughTelegram = signal.NotifyThroughTelegram,

                MaxPrice = price,
                MinPrice = price,

                EntryScoreZ = r.ScoreZ,
                EntryVelocityZ = r.VelocityZ,
                EntryImbalanceZ = r.ImbalanceZ,
                EntryCompressionZ = r.CompressionZ,
                EntryATR = r.ATR,
                EntryER = r.ER,
                EntryFlowState = r.FlowState.State.ToString(),

                Closed = false
            };

            _activeTrades[signal.StrategyName] = trade;

            if (!_replayMode)
                _persistence.Save(_activeTrades);

            _lastEntryTimestamp[signal.StrategyName] = timestamp;
        }

        public void Update(decimal price, long timestamp)
        {
            var trades = _activeTrades.Values.ToList();

            foreach (var trade in trades)
            {
                if (trade.Closed)
                    continue;

                trade.MaxPrice = Math.Max(trade.MaxPrice, price);
                trade.MinPrice = Math.Min(trade.MinPrice, price);

                if (!trade.Target1Hit)
                {
                    bool hit = false;

                    if (trade.Direction == SignalDirection.Long && price >= trade.Target1)
                        hit = true;

                    if (trade.Direction == SignalDirection.Short && price <= trade.Target1)
                        hit = true;

                    if (hit)
                    {
                        trade.Target1Hit = true;
                        trade.RemainingSize *= 0.3m;

                        Target1Reached?.Invoke(trade);
                    }
                }

                if (trade.Direction == SignalDirection.Long && price <= trade.StopLoss)
                {
                    CloseTrade(trade.StrategyName, price, timestamp);
                }

                if (trade.Direction == SignalDirection.Short && price >= trade.StopLoss)
                {
                    CloseTrade(trade.StrategyName, price, timestamp);
                }
            }

            if (!_replayMode)
                _persistence.Save(_activeTrades);
        }

        public void CloseTrade(string strategyName, decimal exitPrice, long timestamp)
        {
            if (!_activeTrades.TryGetValue(strategyName, out var trade))
                return;

            if (trade.Closed)
                return;

            _lastExitTime[strategyName] = timestamp;

            trade.Closed = true;

            _activeTrades.Remove(strategyName);

            _persistence.Save(_activeTrades);

            TradeClosed?.Invoke(trade, exitPrice, timestamp);
        }
    }
}