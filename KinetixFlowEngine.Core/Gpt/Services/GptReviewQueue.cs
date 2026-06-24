using KinetixFlowEngine.Core.Gpt.Models;
using System.Threading.Channels;

namespace KinetixFlowEngine.Core.Gpt.Services;

public interface IGptReviewQueue
{
    Task EnqueueAsync(GptMarketSnapshotV2 snapshot);
}

public sealed class GptReviewQueue : IGptReviewQueue
{
    private readonly Channel<GptMarketSnapshotV2> _channel =
        Channel.CreateUnbounded<GptMarketSnapshotV2>();

    public async Task EnqueueAsync(
        GptMarketSnapshotV2 snapshot)
    {
        await _channel.Writer.WriteAsync(snapshot);
    }

    public ChannelReader<GptMarketSnapshotV2> Reader
        => _channel.Reader;
}