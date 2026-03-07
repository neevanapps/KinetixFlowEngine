using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Flow.Probability;
using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Persistence;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;
using System.Globalization;

namespace KinetixFlowEngine.Core
{
    public class Worker : BackgroundService
    {
        private readonly TradeStreamClient _tradeStreamClient;
        private readonly FlowTradeBuffer _flowTradeBuffer;
        private readonly ILogger<Worker> _logger;
        private readonly KinetixEngineProcessor _engineProcessor;

        private readonly PriceTrendEngine _priceEngine;
        private readonly ScoreTrendEngine _scoreEngine;

        private readonly ScoreNormalizer _scoreNorm;
        private readonly VelocityNormalizer _velNorm;
        private readonly ImbalanceNormalizer _imbNorm;
        private readonly ExhaustionNormalizer _exhNorm;
        private readonly CompressionNormalizer _cmpNorm;

        private readonly SnapshotManager _snapshotManager;

        private readonly OpenInterestClient _openInterestClient;

        private DateTime _lastSnapshot = DateTime.MinValue;
        private DateTime _lastOiFetch = DateTime.MinValue;
        private double _lastOiValue = 0;

        public Worker(FlowTradeBuffer flowTradeBuffer, TradeStreamClient tradeStreamClient, ILogger<Worker> logger, KinetixEngineProcessor engineProcessor, ScoreNormalizer scoreNorm,
                    VelocityNormalizer velNorm, ImbalanceNormalizer imbNorm, ExhaustionNormalizer exhNorm, CompressionNormalizer cmpNorm, SnapshotManager snapshotManager,
                    PriceTrendEngine priceEngine, ScoreTrendEngine scoreEngine, OpenInterestClient openInterestClient)
        {
            _logger = logger;
            _engineProcessor = engineProcessor;

            _flowTradeBuffer = flowTradeBuffer;
            _tradeStreamClient = tradeStreamClient;

            _scoreNorm = scoreNorm;
            _velNorm = velNorm;
            _imbNorm = imbNorm;
            _exhNorm = exhNorm;
            _cmpNorm = cmpNorm;

            _snapshotManager = snapshotManager;

            _priceEngine = priceEngine;
            _scoreEngine = scoreEngine;

            _openInterestClient = openInterestClient;

            _tradeStreamClient.OnTrade += trade =>
            {
                _flowTradeBuffer.AddTrade(trade);
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _tradeStreamClient.StartAsync(stoppingToken);
            var snapshot = _snapshotManager.Load();

            if (snapshot != null)
            {
                var age = DateTime.UtcNow - snapshot.Timestamp;

                if (age.TotalMinutes < 10)
                {
                    _scoreNorm.Restore(snapshot.ScoreNormalizer);
                    _velNorm.Restore(snapshot.VelocityNormalizer);
                    _imbNorm.Restore(snapshot.ImbalanceNormalizer);
                    _exhNorm.Restore(snapshot.ExhaustionNormalizer);
                    _cmpNorm.Restore(snapshot.CompressionNormalizer);

                    _priceEngine.Restore(snapshot.PriceFastEma, snapshot.PriceSlowEma);
                    _scoreEngine.Restore(snapshot.ScoreFastEma, snapshot.ScoreSlowEma);

                    _logger.LogInformation("Snapshot restored successfully.");
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var trades = _flowTradeBuffer.GetSnapshot();
                if (trades == null || trades.Length == 0)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }
                double price = (double)trades[^1].Price;
                if ((DateTime.UtcNow - _lastOiFetch).TotalSeconds > 20)
                {
                    _lastOiValue = await _openInterestClient.GetOpenInterestAsync();
                    _lastOiFetch = DateTime.UtcNow;
                }

                var result = _engineProcessor.Process(price, trades[^1].Quantity, _lastOiValue);

                _logger.LogInformation("FLOW | Price {Price:F1} Score {Score:F1} Fast {Fast:F2} Slow {Slow:F2} Z {ScoreZ:F2} Trend {Trend} State {State} Long {LongProb:F2} Short {ShortProb:F2}",
                                result.Price, result.AdjustedScore, result.ScoreFastEma, result.ScoreSlowEma, result.ScoreZ, result.ScoreTrend, result.FlowState.State, result.LongProbability, result.ShortProbability);

                if ((DateTime.UtcNow - _lastSnapshot).TotalSeconds > 60)
                {
                    _snapshotManager.Save(new EngineSnapshot
                    {
                        Timestamp = DateTime.UtcNow,
                        LastPrice = price,

                        PriceFastEma = _priceEngine.Fast,
                        PriceSlowEma = _priceEngine.Slow,

                        ScoreFastEma = _scoreEngine.Fast,
                        ScoreSlowEma = _scoreEngine.Slow,

                        ScoreNormalizer = _scoreNorm.GetState(),
                        VelocityNormalizer = _velNorm.GetState(),
                        ImbalanceNormalizer = _imbNorm.GetState(),
                        ExhaustionNormalizer = _exhNorm.GetState(),
                        CompressionNormalizer = _cmpNorm.GetState()
                    });

                    _lastSnapshot = DateTime.UtcNow;
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
