using System.Threading.Channels;
using KinetixFlowEngine.Core.Config;
using Microsoft.Extensions.Options;

namespace KinetixFlowEngine.Core.Export;

public sealed class FlowEngineMarketFeatureExportQueue : IFlowEngineMarketFeatureExportQueue
{
    private readonly Channel<FlowEngineMarketFeatureExport> _channel;

    public FlowEngineMarketFeatureExportQueue(IOptions<FlowEngineQuantExportOptions> options)
    {
        var capacity = Math.Max(100, options.Value.MaxQueueSize);
        _channel = Channel.CreateBounded<FlowEngineMarketFeatureExport>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public bool TryEnqueue(FlowEngineMarketFeatureExport feature)
        => _channel.Writer.TryWrite(feature);

    public IAsyncEnumerable<FlowEngineMarketFeatureExport> DequeueAllAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}
