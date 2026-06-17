using Telegram.Bot;
using Telegram.Bot.Types;

namespace KinetixFlowEngine.Core.Utils
{
    public interface INotificationService
    {
        Task SendMessageAsync(string message);
        Task SendGroupMessageAsync(string message);
    }

    public class TelegramService : INotificationService
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _chatId;
        private readonly string _groupChatChatId;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 2000;


        public TelegramService()
        {
            _botClient = new TelegramBotClient("963900714:AAESgesoH3ouwH4e2Wmv7RNa1fxW_rttOiI");
            _chatId = "-1002079360960";
            _groupChatChatId = "747400966";
        }

        public async Task SendMessageAsync(string message)
        {
            _ = Task.Run(async () =>
            {
                await SendWithRetry(message, _chatId);
            });
        }

        private async Task SendWithRetry(string message, string chatId)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    await _botClient.SendMessage(chatId: chatId, text: message);
                    return; // success
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Telegram] Attempt {attempt} failed: {ex.Message}");

                    if (attempt == MaxRetries)
                    {
                        Console.WriteLine("[Telegram] Dropping message after retries.");
                        return;
                    }

                    await Task.Delay(RetryDelayMs);
                }
            }
        }

        public async Task SendGroupMessageAsync(string message)
        {
            _ = Task.Run(async () =>
            {
                await SendWithRetry(message, _groupChatChatId);
            });
        }

        //public async Task SendPhotoAsync(Stream photoStream, string caption = null)
        //{
        //    try
        //    {
        //        // Ensure the stream position is at the beginning
        //        photoStream.Position = 0;

        //        await _botClient.SendPhoto(
        //            chatId: _chatId,
        //            photo: new InputFileStream(photoStream, "screenshot.png"),
        //            caption: caption
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error
        //        Console.WriteLine($"Failed to send Telegram photo: {ex.Message}");
        //        throw;
        //    }
        //}
    }
}
