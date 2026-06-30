namespace KinetixFlowEngine.Core.Export;

public interface IFlowEngineMarketFeatureExportQueue
{
    bool TryEnqueue(FlowEngineMarketFeatureExport feature);
    IAsyncEnumerable<FlowEngineMarketFeatureExport> DequeueAllAsync(CancellationToken cancellationToken);
}
