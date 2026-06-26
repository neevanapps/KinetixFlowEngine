using KinetixFlowEngine.Core.Domain.Liquidity;
using KinetixFlowEngine.Core.Domain.Market;
using KinetixFlowEngine.Core.Domain.Trading;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Models;

namespace KinetixFlowEngine.Core.Flow;

public sealed class MinuteFeaturePipeline
{
    private readonly TradeMinuteBuffer _tradeBuffer;
    private readonly DepthMinuteBuffer _depthBuffer;
    private readonly MinuteCandleBuilder _candleBuilder;
    private readonly IMinuteMarketStateProvider _marketStateProvider;
    private readonly ILogger<MinuteFeaturePipeline> _logger;
    private KinetixEngineResult? _latestEngine;

    public event Action<MinuteMarketContext>? MinuteCompleted;

    public MinuteFeaturePipeline(TradeMinuteBuffer tradeBuffer, DepthMinuteBuffer depthBuffer, MinuteCandleBuilder candleBuilder, IMinuteMarketStateProvider marketStateProvider, ILogger<MinuteFeaturePipeline> logger)
    {
        _tradeBuffer = tradeBuffer;
        _depthBuffer = depthBuffer;
        _candleBuilder = candleBuilder;
        _marketStateProvider = marketStateProvider;
        _logger = logger;
    }

    public void UpdateEngine(KinetixEngineResult result)
    {
        _latestEngine = result;
    }

    public void UpdateDepth(DepthSnapshot snapshot)
    {
        _depthBuffer.AddSnapshot(snapshot);
    }

    public async Task<bool> ProcessTradeAsync(FlowTrade trade, CancellationToken cancellationToken = default)
    {
        //feature = null;

        _tradeBuffer.AddTrade(trade);

        if (!_candleBuilder.TryAddTrade(trade, out var candle))
            return false;

        if (_latestEngine == null)
        {
            _logger.LogWarning("Skipping completed minute because Engine snapshot is not yet available.");
            return false;
        }

        //if (_latestDepth == null)
        //{
        //    _logger.LogWarning("Skipping completed minute because Depth snapshot is not yet available.");
        //    return false;
        //}

        var tradeSnapshot = _tradeBuffer.CompleteMinute(candle!.MinuteUtc);
        var depthSnapshot = _depthBuffer.CompleteMinute(candle.MinuteUtc);
        //feature = _featureBuilder.Build(
        //    candle!,
        //    tradeSnapshot,
        //    _latestDepth,
        //    _latestEngine);

        //_logger.LogInformation(
        //    "Minute {Minute:HH:mm} | O:{Open} H:{High} L:{Low} C:{Close} | Buy:{Buy:F2} Sell:{Sell:F2} BTC | Trades:{Trades}",
        //    feature.TimestampUtc,
        //    feature.Price.Candle.Open,
        //    feature.Price.Candle.High,
        //    feature.Price.Candle.Low,
        //    feature.Price.Candle.Close,
        //    feature.Depth.Liquidity.Bid,
        //    feature.Depth.Liquidity.Ask,
        //    feature.Depth.Pressure.Average);

        //MinuteCompleted?.Invoke(feature);

        //var context = new MinuteMarketContext
        //{
        //    TimestampUtc = candle.MinuteUtc,

        //    Sequence = 0,

        //    EngineBuild = EngineVersion.Version,

        //    Timeframe = MarketTimeframe.OneMinute,

        //    Mode = MarketMode.Live,

        //    Freshness = new DataFreshness(),

        //    PriceCandle = candle,

        //    TradeSnapshot = tradeSnapshot,

        //    DepthSnapshot = depthSnapshot,

        //    Engine = _latestEngine
        //};

        //await _marketStateProvider.CreateAsync(context, cancellationToken);
        _tradeBuffer.Reset();

        //_depthBuffer.Reset();

        return true;
    }
}