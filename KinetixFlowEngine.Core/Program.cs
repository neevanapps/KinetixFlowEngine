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
using Microsoft.Extensions.Hosting.WindowsServices;

namespace KinetixFlowEngine.Core
{
    public static class Program
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "kinetixflowengine-.txt"),
                            rollingInterval: RollingInterval.Day,
                            restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting KinetixFlowEngine");

                var builder = Host.CreateApplicationBuilder(args);

                // Ensure configuration loads from the executable directory
                builder.Configuration.SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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

                var replayMode = builder.Configuration.GetValue<bool>("ReplayMode");
                if (replayMode)
                {
                    var folder = builder.Configuration["ReplayFolder"];
                    builder.Services.AddSingleton<ITradeStreamClient>(new ReplayTradeStreamClient(folder));
                }
                else
                {
                    builder.Services.AddSingleton<ITradeStreamClient, TradeStreamClient>();
                }

                builder.Services.AddSingleton<OpenInterestClient>();
                builder.Services.AddSingleton<TelegramService>();
                builder.Services.AddSingleton<ExceptionAlertAggregator>();

                builder.Services.AddSingleton<FlowTradeBuffer>();
                builder.Services.AddSingleton<FlowAggregationWindow>();
                builder.Services.AddSingleton<FlowFeatureEngine>();
                builder.Services.AddSingleton<FlowCompositeEngine>();
                builder.Services.AddSingleton<FlowScoreEngine>();
                builder.Services.AddSingleton<FlowRegimeEngine>();
                builder.Services.AddSingleton<FlowDivergenceEngine>();
                builder.Services.AddSingleton<FlowProbabilityEngine>();
                builder.Services.AddSingleton<LiquidityPressureEngine>();
                builder.Services.AddSingleton<VwapAbsorptionEngine>();
                builder.Services.AddSingleton<WhaleClusterEngine>();
                builder.Services.AddSingleton<FlowPersistenceEngine>();
                builder.Services.AddSingleton<FlowImpactEngine>();

                builder.Services.AddSingleton<VwapEngine>();
                builder.Services.AddSingleton<EfficiencyRatioEngine>(sp => new EfficiencyRatioEngine(60));
                builder.Services.AddSingleton<EfficiencyRatio30mEngine>();
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
                builder.Services.AddSingleton<ProbabilityTrendEngine>();
                builder.Services.AddSingleton<IKinetixStrategy, ScoreStrategy>();
                builder.Services.AddSingleton<IKinetixStrategy, ProbabilityStrategy>();
                builder.Services.AddSingleton<IKinetixStrategy, HybridStrategy>();
                builder.Services.AddSingleton<IKinetixStrategy, HybridPrice>();
                builder.Services.AddSingleton<StrategyEngine>();
                builder.Services.AddSingleton<StrategyAggregator>();
                builder.Services.AddSingleton<TradePersistence>();
                builder.Services.AddSingleton<PositionManager>();
                builder.Services.AddSingleton<StrategyConfigLoader>();
                builder.Services.AddSingleton<FairPriceEngine>();
                builder.Services.AddSingleton<TradeJournalRecorder>();

                builder.Services.AddHostedService<Worker>();

                // Configure Windows Service lifetime using options because 'Host' is not available on HostApplicationBuilder.
                builder.Services.AddWindowsService(options =>
                {
                    options.ServiceName = "Kinetix Flow Engine";
                });

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
        }
    }
}