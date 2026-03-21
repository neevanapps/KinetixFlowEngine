using KinetixFlowEngine.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Data
{
    public sealed class BybitDepthStreamClient
    {
        private int _started = 0;

        private readonly Uri _uri = new("wss://stream.bybit.com/v5/public/linear");

        private ClientWebSocket? _socket;
        private readonly ILogger<BybitDepthStreamClient> _logger;
        private readonly ExceptionAlertAggregator _exceptionAggregator;

        public event Action<BestPrice>? OnBestPrice;

        private volatile BestPrice _current = new();
        public BestPrice Current => _current;

        public BybitDepthStreamClient(
            ILogger<BybitDepthStreamClient> logger,
            ExceptionAlertAggregator exceptionAggregator)
        {
            _logger = logger;
            _exceptionAggregator = exceptionAggregator;
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

                    _logger.LogWarning("Connecting to Bybit depth stream...");
                    await _socket.ConnectAsync(_uri, ct);

                    _logger.LogInformation("Bybit depth stream connected");

                    await Subscribe(ct);

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
                    _logger.LogError(ex, "Depth WS error. Reconnecting in {delay} ms", retryDelayMs);

                    await Task.Delay(retryDelayMs, ct);
                    retryDelayMs = Math.Min(retryDelayMs * 2, 30000);
                }
            }
        }

        private async Task Subscribe(CancellationToken ct)
        {
            var sub = new
            {
                op = "subscribe",
                args = new[] { "orderbook.50.BTCUSDT" }
            };

            var json = JsonSerializer.Serialize(sub);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _socket!.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                ct);

            _logger.LogInformation("Subscribed to orderbook.50.BTCUSDT");
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
                        _logger.LogWarning("Depth WS closed by server");
                        return;
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
                    _logger.LogError(ex, "Failed to parse depth message");
                }
            }

            _logger.LogWarning("Depth ReceiveLoop exited");
        }

        private void Parse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("topic", out var topic))
                return;

            if (!topic.GetString()!.Contains("orderbook"))
                return;

            if (!root.TryGetProperty("type", out var typeProp))
                return;

            var type = typeProp.GetString();

            if (type != "snapshot" && type != "delta")
                return;

            var data = root.GetProperty("data");

            var bids = data.GetProperty("b");
            var asks = data.GetProperty("a");

            var current = _current;

            var best = new BestPrice
            {
                BestBid = current.BestBid,
                BestAsk = current.BestAsk,
                LastUpdated = current.LastUpdated
            };

            if (bids.GetArrayLength() > 0)
            {
                best.BestBid = decimal.Parse(
                    bids[0][0].GetString()!,
                    CultureInfo.InvariantCulture);
            }

            if (asks.GetArrayLength() > 0)
            {
                best.BestAsk = decimal.Parse(
                    asks[0][0].GetString()!,
                    CultureInfo.InvariantCulture);
            }

            if (best.BestBid == 0 || best.BestAsk == 0)
                return;

            best.LastUpdated = DateTime.UtcNow;

            _current = best;
            OnBestPrice?.Invoke(best);
        }

        public bool IsStale(int seconds = 2)
        {
            return (DateTime.UtcNow - _current.LastUpdated).TotalSeconds > seconds;
        }
    }
    public class BestPrice
    {
        public decimal BestBid { get; set; }
        public decimal BestAsk { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
