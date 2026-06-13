using KinetixFlowEngine.Core.Gpt.Models;
using System.Threading.Channels;

namespace KinetixFlowEngine.Core.Gpt.Services;

public interface IGptReviewQueue
{
    void Enqueue(GptMarketSnapshotV2 snapshot);
}

public sealed class GptReviewQueue : IGptReviewQueue
{
    private readonly Channel<GptMarketSnapshotV2> _channel =
        Channel.CreateUnbounded<GptMarketSnapshotV2>();

    public void Enqueue(
        GptMarketSnapshotV2 snapshot)
    {
        _channel.Writer.TryWrite(snapshot);
    }

    public ChannelReader<GptMarketSnapshotV2> Reader
        => _channel.Reader;
}