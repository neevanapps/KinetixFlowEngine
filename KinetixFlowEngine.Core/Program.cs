using KinetixFlowEngine.Core;
using KinetixFlowEngine.Core.Bootstrap;
using KinetixFlowEngine.Core.Config;
using KinetixFlowEngine.Core.Context;
using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Database;
using KinetixFlowEngine.Core.Database.Repositories;
using KinetixFlowEngine.Core.Database.Serialization;
using KinetixFlowEngine.Core.Depth;
using KinetixFlowEngine.Core.Domain.Common;
using KinetixFlowEngine.Core.Domain.FundingRate;
using KinetixFlowEngine.Core.Domain.Liquidity;
using KinetixFlowEngine.Core.Domain.Market;
using KinetixFlowEngine.Core.Domain.OI;
using KinetixFlowEngine.Core.Domain.Pricing;
using KinetixFlowEngine.Core.Domain.Trading;
using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Execution;
using KinetixFlowEngine.Core.Flow;
using KinetixFlowEngine.Core.Flow.Features;
using KinetixFlowEngine.Core.Flow.Probability;
using KinetixFlowEngine.Core.Flow.State;
using KinetixFlowEngine.Core.Gpt.Configuration;
using KinetixFlowEngine.Core.Gpt.Models;
using KinetixFlowEngine.Core.Gpt.Persistence;
using KinetixFlowEngine.Core.Gpt.Services;
using KinetixFlowEngine.Core.Persistence;
using KinetixFlowEngine.Core.Prop;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Strategy.Strategies;
using KinetixFlowEngine.Core.Trading;
using KinetixFlowEngine.Core.Trend;
using KinetixFlowEngine.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using Serilog.Core;
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
                builder.Services.Configure<GptSettings>(builder.Configuration.GetSection("OpenAI"));
                builder.Services.Configure<CloudAiSettings>(builder.Configuration.GetSection("CloudAI"));
                builder.Services.AddDbContextFactory<KinetixDbContext>(options =>
                {
                    options.UseNpgsql(builder.Configuration.GetConnectionString("KinetixDb"));
                });
                builder.Services.AddHttpClient<OpenInterestClient>(client =>
                {
                    client.BaseAddress = new Uri("https://fapi.binance.com");
                    client.Timeout = TimeSpan.FromSeconds(15);
                });
                builder.Services.AddHttpClient<FundingRateClient>(client =>
                {
                    client.BaseAddress = new Uri("https://api.bybit.com");
                    client.Timeout = TimeSpan.FromSeconds(10);
                });

                //builder.Services.AddScoped<ISnapshotRepository, SnapshotRepository>();
                //builder.Services.AddScoped<IModelReviewRepository, ModelReviewRepository>();
                //builder.Services.AddScoped<IMarketPriceRepository, MarketPriceRepository>();

                builder.Services.AddSingleton<FundingRateEngine>();
                builder.Services.AddSingleton<PropAccountRuntimeFactory>();
                builder.Services.AddSingleton<PropAccountRuntimeManager>();
                builder.Services.AddSingleton<BybitClientFactory>();
                builder.Services.AddSingleton<ExecutionGuard>();
                builder.Services.AddSingleton<BinanceDepthStreamClient>();
                builder.Services.AddSingleton<DepthFeatureEngine>();
                builder.Services.AddSingleton<DepthWallTracker>();

                builder.Services.AddSingleton<MarketMinuteFeatureManager>();
                builder.Services.AddSingleton<MinuteCandleBuilder>();
                builder.Services.AddSingleton<MinuteFeaturePipeline>();

                // Common
                builder.Services.AddSingleton<MetricSeriesBuilder>();

                //builder.Services.AddSingleton(sp =>
                //new MarketDomainServices
                //{
                //    Price = sp.GetRequiredService<PriceFactory>(),
                //    Trade = sp.GetRequiredService<TradeBuilder>(),
                //    Depth = sp.GetRequiredService<DepthBuilder>(),
                //    Funding = sp.GetRequiredService<FundingBuilder>(),
                //    OpenInterest = sp.GetRequiredService<OIBuilder>()
                //});

                builder.Services.AddSingleton<IJsonSerializer<MarketState>, MarketStateSerializer>();

                // Market
                builder.Services.AddSingleton<MarketBuildRequestFactory>();
                builder.Services.AddSingleton<MarketStateFactory>();
                builder.Services.AddSingleton<IMarketStatePipeline, MarketStatePipeline>();
                builder.Services.AddSingleton<IMinuteMarketStateProvider, MinuteMarketStateProvider>();

                builder.Services.AddScoped<IMarketStateRepository, MarketStateRepository>();

                builder.Services.AddSingleton<DepthMinuteBuilder>();

                builder.Services.AddSingleton<PriceBuilder>();
                builder.Services.AddSingleton<PriceSummaryBuilder>();
                builder.Services.AddSingleton<PriceEventBuilder>();
                builder.Services.AddSingleton<PriceFactory>();

                builder.Services.AddSingleton<TradeBuilder>();
                builder.Services.AddSingleton<TradeSummaryBuilder>();
                builder.Services.AddSingleton<TradeEventBuilder>();
                builder.Services.AddSingleton<TradeFactory>();

                builder.Services.AddSingleton<FundingBuilder>();
                builder.Services.AddSingleton<FundingSummaryBuilder>();
                builder.Services.AddSingleton<FundingEventBuilder>();
                builder.Services.AddSingleton<FundingFactory>();

                builder.Services.AddSingleton<OIBuilder>();
                builder.Services.AddSingleton<OISummaryBuilder>();
                builder.Services.AddSingleton<OIEventBuilder>();
                builder.Services.AddSingleton<OIFactory>();

                builder.Services.AddSingleton<MarketEventBuilder>();
                builder.Services.AddSingleton<MarketQualityBuilder>();
                builder.Services.AddSingleton<MarketRegimeBuilder>();
                builder.Services.AddSingleton<MarketSummaryBuilder>();
                builder.Services.AddSingleton<MarketConsensusBuilder>();

                builder.Services.AddSingleton<ExecutionSyncService>();
                builder.Services.AddSingleton<ScoreNormalizer>();
                builder.Services.AddSingleton<VelocityNormalizer>();
                builder.Services.AddSingleton<ImbalanceNormalizer>();
                builder.Services.AddSingleton<ExhaustionNormalizer>();
                builder.Services.AddSingleton<CompressionNormalizer>();
                builder.Services.AddSingleton<AdjustedScoreNormalizer>();
                builder.Services.AddSingleton<FlowImpactNormalizer>();
                builder.Services.AddSingleton<MarketStateManager>();
                builder.Services.AddSingleton<EngineBootstrapService>();
                builder.Services.AddSingleton<EngineWarmupManager>();
                builder.Services.AddSingleton<TradeStreamClient>();
                builder.Services.AddSingleton<BybitDepthStreamClient>();
                builder.Services.AddSingleton<OpenInterestClient>();
                builder.Services.AddSingleton<INotificationService, DiscordWebhookService>();
                builder.Services.AddSingleton<ExceptionAlertAggregator>();
                builder.Services.AddSingleton<VolumeEngine>();
                builder.Services.AddSingleton<AtrNormalizer>();
                builder.Services.AddSingleton<EmaStability>();
                builder.Services.AddSingleton<MarketStructureEngine>();

                builder.Services.AddSingleton<FlowTradeBuffer>();
                builder.Services.AddSingleton<FlowFeatureEngine>();
                builder.Services.AddSingleton<FlowDivergenceEngine>();
                builder.Services.AddSingleton<LiquidityPressureEngine>();
                builder.Services.AddSingleton<VwapAbsorptionEngine>();
                builder.Services.AddSingleton<WhaleClusterEngine>();
                builder.Services.AddSingleton<FlowPersistenceEngine>();
                builder.Services.AddSingleton<FlowImpactEngine>();
                builder.Services.AddSingleton<FlowMomentumRun>();
                builder.Services.AddSingleton<SignalStrengthEngine>();
                builder.Services.AddSingleton<VwapEngine>();
                builder.Services.AddSingleton<FifteenMinuteCandleBuilder>();
                builder.Services.AddSingleton<EfficiencyRatioEngine>(sp => new EfficiencyRatioEngine(14));
                builder.Services.AddSingleton<EfficiencyRatio30mEngine>();
                builder.Services.AddSingleton<AtrEngine>();
                builder.Services.AddSingleton<Atr15mEngine>();
                builder.Services.AddSingleton<OpenInterestEngine>();
                builder.Services.AddSingleton<FlowMetricsRecorder>();
                builder.Services.AddSingleton<SignalStabilityEngine>();
                builder.Services.AddSingleton<PriceTrendEngine>();
                builder.Services.AddSingleton<KinetixEngineProcessor>();
                //builder.Services.AddSingleton<IKinetixStrategy, FastScoreStrategy>();
                //builder.Services.AddSingleton<IKinetixStrategy, MediumScoreStrategy>();
                //builder.Services.AddSingleton<IKinetixStrategy, SlowScoreStrategy>();
                //builder.Services.AddSingleton<IKinetixStrategy, FastProbStrategy>();
                //builder.Services.AddSingleton<IKinetixStrategy, MediumProbStrategy>();
                //builder.Services.AddSingleton<IKinetixStrategy, SlowProbStrategy>();
                //builder.Services.AddSingleton<IKinetixStrategy, FastScorePrice>();
                //builder.Services.AddSingleton<IKinetixStrategy, MediumScorePrice>();
                //builder.Services.AddSingleton<IKinetixStrategy, SlowScorePrice>();
                //builder.Services.AddSingleton<IKinetixStrategy, SlowScoreStrengthStrategy>();
                builder.Services.AddSingleton<StrategyHelper>();
                builder.Services.AddSingleton<IKinetixStrategy, ConsensusStrategy>();
                builder.Services.AddSingleton<IKinetixStrategy, QwenStrategy>();
                builder.Services.AddSingleton<IKinetixStrategy, MistralStrategy>();
                builder.Services.AddSingleton<IKinetixStrategy, GptOssStrategy>();
                builder.Services.AddSingleton<IKinetixStrategy, GlmStrategy>();

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

                builder.Services.AddSingleton<GptSnapshotBuilder>();
                builder.Services.AddSingleton<GptSnapshotStore>();
                builder.Services.AddSingleton<GptMarketStateManager>();
                builder.Services.AddSingleton<GptMultiTimeframeAggregator>();
                builder.Services.AddSingleton<IGptSessionManager, GptSessionManager>();
                builder.Services.AddSingleton<IGptPromptBuilder, GptPromptBuilder>();
                builder.Services.AddSingleton<GptMarketSnapshotV2Builder>();
                
                builder.Services.AddSingleton<IGptReviewStore, GptReviewStore>();
                builder.Services.AddSingleton<ILocalModelReviewer, QwenReviewService>();
                builder.Services.AddSingleton<ILocalModelReviewer, MistralReviewService>();
                builder.Services.AddSingleton<ICloudModelReviewer, GptOssReviewService>();
                builder.Services.AddSingleton<ICloudModelReviewer, GlmResvireService>();
                //builder.Services.AddSingleton<ICloudModelReviewer, DeepSeekReviewService>();
                //builder.Services.AddSingleton<ICloudModelReviewer, NemotronReviewService>();

                builder.Services.AddHostedService<ReviewMemoryWarmupService>();
                builder.Services.AddSingleton<CompositeReviewService>();
                builder.Services.AddSingleton<GptReviewQueue>();
                builder.Services.AddSingleton<IGptReviewQueue>(sp => sp.GetRequiredService<GptReviewQueue>());
                builder.Services.AddHostedService<GptReviewBackgroundService>();
                builder.Services.AddSingleton<LlmReviewMemory>();
                builder.Services.AddSingleton<ReviewSmoothingService>();
                builder.Services.AddSingleton<ConsensusReviewService>();
                builder.Services.AddSingleton<ModelReviewProvider>();
                builder.Services.AddSingleton<ConsensusProvider>();
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