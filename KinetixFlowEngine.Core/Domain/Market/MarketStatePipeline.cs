using KinetixFlowEngine.Core.Database.Repositories;

namespace KinetixFlowEngine.Core.Domain.Market;

public interface IMarketStatePipeline
{
    Task<MarketState> CreateAsync(
        MarketBuildRequest request,
        CancellationToken cancellationToken = default);
}
public sealed class MarketStatePipeline : IMarketStatePipeline
{
    private readonly MarketStateFactory _factory;
    private readonly IMarketStateRepository _repository;

    public MarketStatePipeline(
        MarketStateFactory factory,
        IMarketStateRepository repository)
    {
        _factory = factory;
        _repository = repository;
    }

    public async Task<MarketState> CreateAsync(
    MarketBuildRequest request,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var state = _factory.Build(request);

        await _repository.SaveAsync(
            state,
            cancellationToken);

        return state;
    }

    
}