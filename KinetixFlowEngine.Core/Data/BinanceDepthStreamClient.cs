using KinetixFlowEngine.Core.Domain.Liquidity;
using KinetixFlowEngine.Core.Utils;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Data;

public sealed class BinanceDepthStreamClient
{
    private readonly Uri _uri =
        new("wss://fstream.binance.com/ws/btcusdt@depth20@100ms");

    private readonly ILogger<BinanceDepthStreamClient> _logger;
    private readonly ExceptionAlertAggregator _exceptionAggregator;

    private ClientWebSocket? _socket;

    private int _started;

    public DepthSnapshot CurrentSnapshot { get; private set; }
        = new();

    public BinanceDepthStreamClient(
        ILogger<BinanceDepthStreamClient> logger,
        ExceptionAlertAggregator exceptionAggregator)
    {
        _logger = logger;
        _exceptionAggregator = exceptionAggregator;
    }

    public async Task StartAsync(
        CancellationToken ct)
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
            return;

        _ = Task.Run(() => RunAsync(ct));
    }

    private async Task RunAsync(
        CancellationToken ct)
    {
        var retryDelayMs = 2000;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                _socket?.Dispose();

                _socket = new ClientWebSocket();

                _socket.Options.KeepAliveInterval =
                    TimeSpan.FromSeconds(30);

                _logger.LogWarning(
                    "Connecting to Binance depth stream...");

                await _socket.ConnectAsync(
                    _uri,
                    ct);

                _logger.LogInformation(
                    "Binance depth stream connected");

                retryDelayMs = 2000;

                await ReceiveLoop(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _exceptionAggregator.Capture(ex);

                _logger.LogError(
                    ex,
                    "Depth stream error. Reconnecting in {Delay} ms",
                    retryDelayMs);

                await Task.Delay(
                    retryDelayMs,
                    ct);

                retryDelayMs =
                    Math.Min(retryDelayMs * 2, 30000);
            }
        }
    }

    private async Task ReceiveLoop(
        CancellationToken ct)
    {
        var buffer = new byte[16384];

        while (!ct.IsCancellationRequested &&
               _socket?.State == WebSocketState.Open)
        {
            using var ms = new MemoryStream();

            WebSocketReceiveResult result;

            do
            {
                result =
                    await _socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        ct);

                if (result.MessageType ==
                    WebSocketMessageType.Close)
                {
                    _logger.LogWarning(
                        "Depth WS closed by server");

                    return;
                }

                ms.Write(
                    buffer,
                    0,
                    result.Count);

            }
            while (!result.EndOfMessage);

            var json =
                Encoding.UTF8.GetString(
                    ms.ToArray());

            try
            {
                Parse(json);
            }
            catch (Exception ex)
            {
                _exceptionAggregator.Capture(ex);

                _logger.LogError(
                    ex,
                    "Failed to parse depth message");
            }
        }

        _logger.LogWarning(
            "Depth ReceiveLoop exited");
    }

    private void Parse(
        string json)
    {
        using var doc =
            JsonDocument.Parse(json);

        var root =
            doc.RootElement;

        var bids =
            ParseLevels(
                root.GetProperty("b"));

        var asks =
            ParseLevels(
                root.GetProperty("a"));

        CurrentSnapshot =
            new DepthSnapshot
            {
                TimestampUtc = DateTime.UtcNow,
                Bids = bids,
                Asks = asks
            };
    }

    private static IReadOnlyList<DepthLevel>
        ParseLevels(
            JsonElement levels)
    {
        var result =
            new List<DepthLevel>(
                levels.GetArrayLength());

        foreach (var level in levels.EnumerateArray())
        {
            var price =
                decimal.Parse(
                    level[0].GetString()!,
                    CultureInfo.InvariantCulture);

            var qty =
                decimal.Parse(
                    level[1].GetString()!,
                    CultureInfo.InvariantCulture);

            result.Add(
                new DepthLevel
                {
                    Price = price,
                    Quantity = qty
                });
        }

        return result;
    }
}