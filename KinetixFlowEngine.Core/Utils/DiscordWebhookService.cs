using System.Text;
using System.Text.Json;

namespace KinetixFlowEngine.Core.Utils
{

    public class DiscordWebhookService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _personalWebhookUrl;
        private readonly string _groupWebhookUrl;

        private const int MaxRetries = 3;
        private const int RetryDelayMs = 2000;

        public DiscordWebhookService()
        {
            _httpClient = new HttpClient();

            // === REPLACE THESE WITH YOUR DISCORD WEBHOOK URLs ===
            _personalWebhookUrl = "https://discord.com/api/webhooks/1516630491539836998/6ury8bB2xW2h238W3PYz7PpWoLCrLMeU33lcY7wbB3lF3uGR1QASQKCUcaNlpe15MQ_e";
            _groupWebhookUrl = "https://discord.com/api/webhooks/1516630748986216560/rBNzEtHGUnp-GlW609msBlQd0D37DnkF4PulyUFWn41jN0nb6T4rf7j8bCNWZ1yByEQ1";
        }

        public async Task SendMessageAsync(string message)
        {
            _ = Task.Run(async () =>
            {
                await SendWithRetryAsync(message, _personalWebhookUrl);
            });
        }

        public async Task SendGroupMessageAsync(string message)
        {
            _ = Task.Run(async () =>
            {
                await SendWithRetryAsync(message, _groupWebhookUrl);
            });
        }

        private async Task SendWithRetryAsync(string message, string webhookUrl)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var payload = new
                    {
                        content = message.Length > 2000 ? message.Substring(0, 1997) + "..." : message
                    };

                    var json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(webhookUrl, content);

                    if (response.IsSuccessStatusCode)
                        return;

                    Console.WriteLine($"[Discord] Attempt {attempt} failed: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Discord] Attempt {attempt} failed: {ex.Message}");
                }

                if (attempt == MaxRetries)
                {
                    Console.WriteLine("[Discord] Dropping message after retries.");
                    return;
                }

                await Task.Delay(RetryDelayMs);
            }
        }
    }
}