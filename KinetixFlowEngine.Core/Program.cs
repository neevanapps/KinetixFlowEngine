using KinetixFlowEngine.Core;
using KinetixFlowEngine.Core.Bootstrap;
using KinetixFlowEngine.Core.Config;
using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Flow.Probability;
using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Persistence;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Strategy.Strategies;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/kinetixflowengine-.txt",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting KinetixFlowEngine");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger, dispose: true);

    builder.Services.Configure<FlowEngineOptions>(builder.Configuration.GetSection("FlowEngine"));
    builder.Services.Configure<NormalizationOptions>(builder.Configuration.GetSection("Normalization"));

    builder.Services.AddHttpClient<OpenInterestClient>(client =>
    {
        client.BaseAddress = new Uri("https://fapi.binance.com");
        client.Timeout = TimeSpan.FromSeconds(15);
    });

    builder.Services.AddSingleton<ScoreNormalizer>();
    builder.Services.AddSingleton<VelocityNormalizer>();
    builder.Services.AddSingleton<ImbalanceNormalizer>();
    builder.Services.AddSingleton<ExhaustionNormalizer>();
    builder.Services.AddSingleton<CompressionNormalizer>();
    builder.Services.AddSingleton<MarketStateManager>();
    builder.Services.AddSingleton<EngineBootstrapService>();
    builder.Services.AddSingleton<EngineWarmupManager>();
    builder.Services.AddSingleton<TradeStreamClient>();
    builder.Services.AddSingleton<OpenInterestClient>();
    builder.Services.AddSingleton<TelegramService>();
    builder.Services.AddSingleton<ExceptionAlertAggregator>();

    builder.Services.AddSingleton<FlowTradeBuffer>();
    builder.Services.AddSingleton<FlowAggregationWindow>();
    builder.Services.AddSingleton<FlowFeatureEngine>();
    builder.Services.AddSingleton<FlowCompositeEngine>();
    builder.Services.AddSingleton<FlowScoreEngine>();
    builder.Services.AddSingleton<FlowRegimeEngine>();
    builder.Services.AddSingleton<FlowProbabilityEngine>();

    builder.Services.AddSingleton<VwapEngine>();
    builder.Services.AddSingleton<EfficiencyRatioEngine>();
    builder.Services.AddSingleton<AtrEngine>();  // 1m ATR
    builder.Services.AddSingleton<Atr15mEngine>();   // 15m ATR
    builder.Services.AddSingleton<OpenInterestEngine>();
    builder.Services.AddSingleton<ContextScoreEngine>();
    builder.Services.AddSingleton<FlowMetricsRecorder>();
    builder.Services.AddSingleton<SignalStabilityEngine>();
    builder.Services.AddSingleton<PriceTrendEngine>();
    builder.Services.AddSingleton<ScoreTrendEngine>();
    builder.Services.AddSingleton<FlowStateEngine>();
    builder.Services.AddSingleton<KinetixEngineProcessor>();

    builder.Services.AddSingleton<IKinetixStrategy, FlowMomentumStrategy>();
    builder.Services.AddSingleton<StrategyEngine>();
    builder.Services.AddSingleton<StrategyAggregator>();
    builder.Services.AddSingleton<TradePersistence>();
    builder.Services.AddSingleton<PositionManager>();
    builder.Services.AddSingleton<StrategyConfigLoader>();
    builder.Services.AddSingleton<FairPriceEngine>();

    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}