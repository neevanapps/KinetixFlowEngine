using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Quant;

public sealed class QuantModelConsensusCache : BackgroundService, IQuantModelConsensusProvider
{
    private readonly IQuantModelConsensusService _consensusService;
    private readonly QuantModelConsensusCacheOptions _options;
    private readonly ILogger<QuantModelConsensusCache> _logger;

    private readonly object _sync = new();
    private QuantModelConsensusDecision? _latest;

    public QuantModelConsensusCache(
        IQuantModelConsensusService consensusService,
        IOptions<QuantModelConsensusCacheOptions> options,
        ILogger<QuantModelConsensusCache> logger)
    {
        _consensusService = consensusService;
        _options = options.Value;
        _logger = logger;
    }

    public QuantModelConsensusDecision? GetLatest()
    {
        lock (_sync)
        {
            return _latest;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("Quant model consensus cache is disabled.");
            return;
        }

        var refreshInterval = TimeSpan.FromSeconds(
            Math.Max(5, _options.RefreshIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consensus = await _consensusService.GetLatestConsensusAsync(stoppingToken);

                lock (_sync)
                {
                    _latest = consensus;
                }

                if (_options.LogRefreshResult)
                {
                    _logger.LogInformation(
                        "Quant temporal consensus refreshed | Available={Available} | CurrentPayload={CurrentPayload} | PreviousPayload={PreviousPayload} | ThirdPayload={ThirdPayload} | CurrentDirection={CurrentDirection} | PreviousDirection={PreviousDirection} | ThirdDirection={ThirdDirection} | ThreeAgree={ThreeAgree} | TemporalDirection={TemporalDirection} | Action={Action} | ShouldTrade={ShouldTrade} | TemporalScore={TemporalScore} | CurrentScore={CurrentScore} | DirectionalAgreement={DirectionalAgreement} | ExecutableVotes={ExecutableVotes} | ExecutableBatches={ExecutableBatches} | SpanMinutes={SpanMinutes} | Stability={Stability} | Block={Block}",
                        consensus.IsAvailable,
                        consensus.CurrentPayloadId,
                        consensus.PreviousPayloadId,
                        consensus.ThirdPayloadId,
                        consensus.CurrentBatchDirection,
                        consensus.PreviousBatchDirection,
                        consensus.ThirdBatchDirection,
                        consensus.ThreeDirectionsAgree,
                        consensus.Direction,
                        consensus.RecommendedAction,
                        consensus.ShouldTrade,
                        consensus.WeightedDirectionalScore,
                        consensus.CurrentBatchWeightedDirectionalScore,
                        consensus.CurrentBatchDirectionalAgreementRatio,
                        consensus.CurrentBatchExecutableVoteCount,
                        consensus.ExecutableBatchCount,
                        consensus.TemporalSpanMinutes,
                        consensus.Stability,
                        consensus.BlockReason);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh Quant model consensus cache.");
            }

            await Task.Delay(refreshInterval, stoppingToken);
        }
    }
}

public interface IQuantModelConsensusProvider
{
    QuantModelConsensusDecision? GetLatest();
}
