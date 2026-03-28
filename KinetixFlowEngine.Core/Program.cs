using KinetixFlowEngine.Core;
using KinetixFlowEngine.Core.Bootstrap;
using KinetixFlowEngine.Core.Config;
using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Execution;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Flow.Probability;
using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Persistence;
using KinetixFlowEngine.Core.Prop;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Strategy.Strategies;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using Serilog.Events;

namespace KinetixFlowEngine.Core
{
    public static class Program
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient.*", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "kinetixflowengine-.txt"),
                            rollingInterval: RollingInterval.Day,
                            restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();

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
                builder.Services.Configure<PropAccountsOptions>(builder.Configuration.GetSection("PropAccounts"));

                builder.Services.AddHttpClient<OpenInterestClient>(client =>
                {
                    client.BaseAddress = new Uri("https://fapi.binance.com");
                    client.Timeout = TimeSpan.FromSeconds(15);
                });
                builder.Services.AddSingleton<PropAccountRuntimeFactory>();
                builder.Services.AddSingleton<PropAccountRuntimeManager>();
                builder.Services.AddSingleton<BybitClientFactory>();
                builder.Services.AddSingleton<ExecutionGuard>();

                builder.Services.AddSingleton<ExecutionSyncService>();
                builder.Services.AddSingleton<ScoreNormalizer>();
                builder.Services.AddSingleton<VelocityNormalizer>();
                builder.Services.AddSingleton<ImbalanceNormalizer>();
                builder.Services.AddSingleton<ExhaustionNormalizer>();
                builder.Services.AddSingleton<CompressionNormalizer>();
                builder.Services.AddSingleton<AdjustedScoreNormalizer>();
                builder.Services.AddSingleton<MarketStateManager>();
                builder.Services.AddSingleton<EngineBootstrapService>();
                builder.Services.AddSingleton<EngineWarmupManager>();
                builder.Services.AddSingleton<TradeStreamClient>();
                builder.Services.AddSingleton<BybitDepthStreamClient>();
                builder.Services.AddSingleton<OpenInterestClient>();
                builder.Services.AddSingleton<TelegramService>();
                builder.Services.AddSingleton<ExceptionAlertAggregator>();
                builder.Services.AddSingleton<VolumeEngine>();
                builder.Services.AddSingleton<EmaStability>();

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
                builder.Services.AddSingleton<FlowMomentumRun>();

                builder.Services.AddSingleton<VwapEngine>();
                builder.Services.AddSingleton<FifteenMinuteCandleBuilder>();
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
                builder.Services.AddSingleton<IKinetixStrategy, ExpansionBreakoutStrategy>();
                builder.Services.AddSingleton<IKinetixStrategy, PullbackContinuationStrategy>();
                builder.Services.AddSingleton<IKinetixStrategy, TrendCoreStrategy>();
                builder.Services.AddSingleton<StrategyEngine>();
                builder.Services.AddSingleton<StrategyAggregator>();
                builder.Services.AddSingleton<TradePersistence>();
                builder.Services.AddSingleton<PositionPersistence>();
                builder.Services.AddSingleton<PositionManager>();
                builder.Services.AddSingleton<StrategyConfigLoader>();
                builder.Services.AddSingleton<FairPriceEngine>();
                builder.Services.AddSingleton<TradeJournalRecorder>();
                builder.Services.AddSingleton<TradeMemoryManager>();
                builder.Services.AddSingleton<PropAlertService>();
                builder.Services.AddSingleton<PropAccountStatePersistence>();
                builder.Services.AddHostedService<Worker>();
                builder.Services.AddSingleton<ITradeExecutor, SimulatedExecutor>();
                builder.Services.AddSingleton<ITradeExecutor, BybitExecutor>();
                builder.Services.AddSingleton<IExecutionRouter, ExecutionRouter>();
                builder.Services.AddSingleton<ISimExecutionPipeline, SimExecutionPipeline>();
                builder.Services.AddSingleton<IAccountExecutionPipeline, AccountExecutionPipeline>();
                builder.Services.AddSingleton<IEquityEngine, EquityEngine>();
                builder.Services.AddSingleton<IPositionSizer, PropPositionSizer>();
                builder.Services.AddSingleton<AccountStateEngine>();
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