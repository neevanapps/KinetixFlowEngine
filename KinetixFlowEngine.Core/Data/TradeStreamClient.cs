using KinetixFlowEngine.Core.Data;
using KinetixFlowEngine.Core.Models;
using KinetixFlowEngine.Core.Utils;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public sealed class TradeStreamClient : ITradeStreamClient
{
    private int _started = 0;
    private readonly Uri _uri = new("wss://fstream.binance.com/ws/btcusdt@aggTrade");

    private ClientWebSocket? _socket;
    private readonly ILogger<TradeStreamClient> _logger;
    private readonly ExceptionAlertAggregator _exceptionAggregator;
    public event Action<FlowTrade>? OnTrade;

    public TradeStreamClient(ILogger<TradeStreamClient> logger, ExceptionAlertAggregator exceptionAggregator)
    {
        _logger = logger;
        _exceptionAggregator = exceptionAggregator;
    }

    public void EmitReplayTrade(FlowTrade trade)
    {
        OnTrade?.Invoke(trade);
    }

    public async Task StartAsync(CancellationToken ct)
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
            return;

        _ = Task.Run(() => RunAsync(ct));
    }

    private async Task RunAsync(CancellationToken ct)
    {
        int retryDelayMs = 2000;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                _socket?.Dispose();
                _socket = new ClientWebSocket();

                _logger.LogWarning("Connecting to trade stream...");
                await _socket.ConnectAsync(_uri, ct);

                _logger.LogInformation("Trade stream connected");

                retryDelayMs = 2000; // reset backoff
                await ReceiveLoop(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _exceptionAggregator.Capture(ex);
                _logger.LogError(ex, "Trade stream error. Reconnecting in {delay} ms", retryDelayMs);
                await Task.Delay(retryDelayMs, ct);
                retryDelayMs = Math.Min(retryDelayMs * 2, 30000);
            }
        }
    }

    private async Task ReceiveLoop(CancellationToken ct)
    {
        var buffer = new byte[8192];

        while (!ct.IsCancellationRequested && _socket?.State == WebSocketState.Open)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await _socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogWarning("Trade WS closed by server");
                    return; // triggers reconnect
                }

                ms.Write(buffer, 0, result.Count);

            } while (!result.EndOfMessage);

            var json = Encoding.UTF8.GetString(ms.ToArray());

            try
            {
                Parse(json);
            }
            catch (Exception ex)
            {
                _exceptionAggregator.Capture(ex);
                _logger.LogError(ex, "Failed to parse trade WS message");
            }
        }

        _logger.LogWarning("Trade ReceiveLoop exited");
    }

    private void Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var qty = decimal.Parse(
            root.GetProperty("q").GetString()!,
            CultureInfo.InvariantCulture);
        var prc = decimal.Parse(
            root.GetProperty("p").GetString()!,
            CultureInfo.InvariantCulture);

        var isSellTaker = root.GetProperty("m").GetBoolean();
        var ts = root.GetProperty("T").GetInt64();

        var trade = new FlowTrade
        {
            Quantity = qty,
            Price = prc,
            IsBuyerMaker = isSellTaker,
            Timestamp = ts
        };

        _logger.LogDebug("Emitting trade: {Price}", prc); // Added for event debugging
        OnTrade?.Invoke(trade);
    }
}