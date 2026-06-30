using KinetixFlowEngine.Core.Config;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;

namespace KinetixFlowEngine.Core.Export;

public sealed class FlowEngineMarketFeaturePostgresWriter : BackgroundService
{
    private readonly IFlowEngineMarketFeatureExportQueue _queue;
    private readonly IConfiguration _configuration;
    private readonly FlowEngineQuantExportOptions _options;
    private readonly ILogger<FlowEngineMarketFeaturePostgresWriter> _logger;

    public FlowEngineMarketFeaturePostgresWriter(
        IFlowEngineMarketFeatureExportQueue queue,
        IConfiguration configuration,
        IOptions<FlowEngineQuantExportOptions> options,
        ILogger<FlowEngineMarketFeaturePostgresWriter> logger)
    {
        _queue = queue;
        _configuration = configuration;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("FlowEngine Quant export disabled.");
            return;
        }

        var connectionString = _configuration.GetConnectionString(_options.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("FlowEngine Quant export enabled, but connection string '{Name}' is missing.", _options.ConnectionStringName);
            return;
        }

        await foreach (var feature in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await UpsertAsync(connectionString, feature, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed exporting Flow feature to Quant DB. Symbol={Symbol}, Timestamp={Timestamp}",
                    feature.Symbol, feature.TimestampUtc);

                var delay = TimeSpan.FromSeconds(Math.Max(1, _options.RetryDelaySeconds));
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    private static async Task UpsertAsync(
        string connectionString,
        FlowEngineMarketFeatureExport f,
        CancellationToken ct)
    {
        const string sql = """
INSERT INTO flow_engine_market_features (
    feature_id, symbol, timestamp_utc, created_utc, received_utc,
    price, price_change_1m, vwap, distance_from_vwap_pct, atr_15m,
    score_z, velocity_z, imbalance_z, compression_z, exhaustion_z,
    momentum, acceleration, persistence, net_pressure, flow_impact_efficiency,
    avg_depth_imbalance_top10, bullish_book_percent, bearish_book_percent,
    bullish_persistence_seconds, bearish_persistence_seconds,
    largest_bid_wall_age_sec, largest_ask_wall_age_sec,
    largest_bid_wall_qty, largest_ask_wall_qty,
    consumed_bid_wall_count, consumed_ask_wall_count,
    avg_bid_quantity_change_pct, avg_ask_quantity_change_pct,
    spread_bps, best_bid_price, best_ask_price,
    top_bid_qty, top_ask_qty, top10_bid_qty, top10_ask_qty,
    raw_json
)
VALUES (
    @feature_id, @symbol, @timestamp_utc, @created_utc, now(),
    @price, @price_change_1m, @vwap, @distance_from_vwap_pct, @atr_15m,
    @score_z, @velocity_z, @imbalance_z, @compression_z, @exhaustion_z,
    @momentum, @acceleration, @persistence, @net_pressure, @flow_impact_efficiency,
    @avg_depth_imbalance_top10, @bullish_book_percent, @bearish_book_percent,
    @bullish_persistence_seconds, @bearish_persistence_seconds,
    @largest_bid_wall_age_sec, @largest_ask_wall_age_sec,
    @largest_bid_wall_qty, @largest_ask_wall_qty,
    @consumed_bid_wall_count, @consumed_ask_wall_count,
    @avg_bid_quantity_change_pct, @avg_ask_quantity_change_pct,
    @spread_bps, @best_bid_price, @best_ask_price,
    @top_bid_qty, @top_ask_qty, @top10_bid_qty, @top10_ask_qty,
    @raw_json::jsonb
)
ON CONFLICT (symbol, timestamp_utc) DO UPDATE SET
    feature_id = EXCLUDED.feature_id,
    created_utc = EXCLUDED.created_utc,
    received_utc = now(),
    price = EXCLUDED.price,
    price_change_1m = EXCLUDED.price_change_1m,
    vwap = EXCLUDED.vwap,
    distance_from_vwap_pct = EXCLUDED.distance_from_vwap_pct,
    atr_15m = EXCLUDED.atr_15m,
    score_z = EXCLUDED.score_z,
    velocity_z = EXCLUDED.velocity_z,
    imbalance_z = EXCLUDED.imbalance_z,
    compression_z = EXCLUDED.compression_z,
    exhaustion_z = EXCLUDED.exhaustion_z,
    momentum = EXCLUDED.momentum,
    acceleration = EXCLUDED.acceleration,
    persistence = EXCLUDED.persistence,
    net_pressure = EXCLUDED.net_pressure,
    flow_impact_efficiency = EXCLUDED.flow_impact_efficiency,
    avg_depth_imbalance_top10 = EXCLUDED.avg_depth_imbalance_top10,
    bullish_book_percent = EXCLUDED.bullish_book_percent,
    bearish_book_percent = EXCLUDED.bearish_book_percent,
    bullish_persistence_seconds = EXCLUDED.bullish_persistence_seconds,
    bearish_persistence_seconds = EXCLUDED.bearish_persistence_seconds,
    largest_bid_wall_age_sec = EXCLUDED.largest_bid_wall_age_sec,
    largest_ask_wall_age_sec = EXCLUDED.largest_ask_wall_age_sec,
    largest_bid_wall_qty = EXCLUDED.largest_bid_wall_qty,
    largest_ask_wall_qty = EXCLUDED.largest_ask_wall_qty,
    consumed_bid_wall_count = EXCLUDED.consumed_bid_wall_count,
    consumed_ask_wall_count = EXCLUDED.consumed_ask_wall_count,
    avg_bid_quantity_change_pct = EXCLUDED.avg_bid_quantity_change_pct,
    avg_ask_quantity_change_pct = EXCLUDED.avg_ask_quantity_change_pct,
    spread_bps = EXCLUDED.spread_bps,
    best_bid_price = EXCLUDED.best_bid_price,
    best_ask_price = EXCLUDED.best_ask_price,
    top_bid_qty = EXCLUDED.top_bid_qty,
    top_ask_qty = EXCLUDED.top_ask_qty,
    top10_bid_qty = EXCLUDED.top10_bid_qty,
    top10_ask_qty = EXCLUDED.top10_ask_qty,
    raw_json = EXCLUDED.raw_json;
""";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, connection);

        Add(cmd, "feature_id", f.FeatureId);
        Add(cmd, "symbol", f.Symbol);
        Add(cmd, "timestamp_utc", f.TimestampUtc);
        Add(cmd, "created_utc", f.CreatedUtc);
        Add(cmd, "price", f.Price);
        Add(cmd, "price_change_1m", f.PriceChange1m);
        Add(cmd, "vwap", f.Vwap);
        Add(cmd, "distance_from_vwap_pct", f.DistanceFromVwapPct);
        Add(cmd, "atr_15m", f.Atr15m);
        Add(cmd, "score_z", f.ScoreZ);
        Add(cmd, "velocity_z", f.VelocityZ);
        Add(cmd, "imbalance_z", f.ImbalanceZ);
        Add(cmd, "compression_z", f.CompressionZ);
        Add(cmd, "exhaustion_z", f.ExhaustionZ);
        Add(cmd, "momentum", f.Momentum);
        Add(cmd, "acceleration", f.Acceleration);
        Add(cmd, "persistence", f.Persistence);
        Add(cmd, "net_pressure", f.NetPressure);
        Add(cmd, "flow_impact_efficiency", f.FlowImpactEfficiency);
        Add(cmd, "avg_depth_imbalance_top10", f.AvgDepthImbalanceTop10);
        Add(cmd, "bullish_book_percent", f.BullishBookPercent);
        Add(cmd, "bearish_book_percent", f.BearishBookPercent);
        Add(cmd, "bullish_persistence_seconds", f.BullishPersistenceSeconds);
        Add(cmd, "bearish_persistence_seconds", f.BearishPersistenceSeconds);
        Add(cmd, "largest_bid_wall_age_sec", f.LargestBidWallAgeSec);
        Add(cmd, "largest_ask_wall_age_sec", f.LargestAskWallAgeSec);
        Add(cmd, "largest_bid_wall_qty", f.LargestBidWallQty);
        Add(cmd, "largest_ask_wall_qty", f.LargestAskWallQty);
        Add(cmd, "consumed_bid_wall_count", f.ConsumedBidWallCount);
        Add(cmd, "consumed_ask_wall_count", f.ConsumedAskWallCount);
        Add(cmd, "avg_bid_quantity_change_pct", f.AvgBidQuantityChangePct);
        Add(cmd, "avg_ask_quantity_change_pct", f.AvgAskQuantityChangePct);
        Add(cmd, "spread_bps", f.SpreadBps);
        Add(cmd, "best_bid_price", f.BestBidPrice);
        Add(cmd, "best_ask_price", f.BestAskPrice);
        Add(cmd, "top_bid_qty", f.TopBidQty);
        Add(cmd, "top_ask_qty", f.TopAskQty);
        Add(cmd, "top10_bid_qty", f.Top10BidQty);
        Add(cmd, "top10_ask_qty", f.Top10AskQty);
        cmd.Parameters.AddWithValue("raw_json", NpgsqlDbType.Jsonb, string.IsNullOrWhiteSpace(f.RawJson) ? "{}" : f.RawJson);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void Add(NpgsqlCommand cmd, string name, object? value)
        => cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
}
