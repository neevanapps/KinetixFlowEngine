using System.Collections.Concurrent;
using System.Text;

namespace KinetixFlowEngine.Core.Utils
{
    public sealed class ExceptionAlertAggregator : IDisposable
    {
        private readonly ConcurrentDictionary<string, int> _exceptionCounts = new();
        private readonly TimeSpan _flushInterval = TimeSpan.FromMinutes(5);
        private readonly PeriodicTimer _timer;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ExceptionAlertAggregator> _logger;
        private readonly CancellationTokenSource _cts = new();

        private const int MAX_DISTINCT_EXCEPTIONS = 200;
        private const int MAX_MESSAGE_LENGTH = 3500;

        private DateTime _lastFlush = DateTime.UtcNow;

        public ExceptionAlertAggregator(
            INotificationService notificationService,
            ILogger<ExceptionAlertAggregator> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            _ = Task.Run(FlushLoopAsync);
        }

        public void Capture(Exception ex)
        {
            if (ex == null) return;

            try
            {
                var key = $"{ex.GetType().Name}: {ex.Message}";

                if (_exceptionCounts.Count >= MAX_DISTINCT_EXCEPTIONS)
                {
                    _exceptionCounts.AddOrUpdate(
                        "TooManyDistinctExceptions",
                        1,
                        (_, count) => count + 1);
                    return;
                }

                _exceptionCounts.AddOrUpdate(key, 1, (_, count) => count + 1);
            }
            catch
            {
                // NEVER allow exception capture to crash engine
            }
        }

        private async Task FlushLoopAsync()
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    if (DateTime.UtcNow - _lastFlush < _flushInterval)
                        continue;

                    if (_exceptionCounts.IsEmpty)
                        continue;

                    await FlushInternalAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception aggregator loop failed");
            }
        }

        private async Task FlushInternalAsync()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("⚠️ ENGINE EXCEPTIONS (Last 5 Minutes)");
                sb.AppendLine("------------------------------------");

                foreach (var kvp in _exceptionCounts.OrderByDescending(x => x.Value))
                {
                    sb.AppendLine($"{kvp.Key}");
                    sb.AppendLine($"Count: {kvp.Value}");
                    sb.AppendLine();

                    if (sb.Length >= MAX_MESSAGE_LENGTH)
                    {
                        sb.AppendLine("... Truncated ...");
                        break;
                    }
                }

                var message = sb.ToString();

                await _notificationService.SendMessageAsync(message);

                _exceptionCounts.Clear();
                _lastFlush = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to flush exception summary to Telegram");
                // DO NOT clear counts if sending failed
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _timer.Dispose();
            _cts.Dispose();
        }
    }
}
