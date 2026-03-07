using Telegram.Bot;
using Telegram.Bot.Types;

namespace KinetixFlowEngine.Core.Utils
{
    public class TelegramService
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _chatId;
        private readonly string _groupChatChatId;

        public TelegramService()
        {
            _botClient = new TelegramBotClient("963900714:AAESgesoH3ouwH4e2Wmv7RNa1fxW_rttOiI");
            _chatId = "-1002079360960";
            _groupChatChatId = "747400966";
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                await _botClient.SendMessage(chatId: _chatId, text: message);
            }
            catch (Exception ex)
            {
                // Log the error (you can replace this with your logging mechanism)
                Console.WriteLine($"Failed to send Telegram message: {ex.Message}");
                throw;
            }
        }

        public async Task SendGroupMessageAsync(string message)
        {
            try
            {
                await _botClient.SendMessage(chatId: _groupChatChatId, text: message);
            }
            catch (Exception ex)
            {
                // Log the error (you can replace this with your logging mechanism)
                Console.WriteLine($"Failed to send Telegram message: {ex.Message}");
                throw;
            }
        }

        public async Task SendPhotoAsync(Stream photoStream, string caption = null)
        {
            try
            {
                // Ensure the stream position is at the beginning
                photoStream.Position = 0;

                await _botClient.SendPhoto(
                    chatId: _chatId,
                    photo: new InputFileStream(photoStream, "screenshot.png"),
                    caption: caption
                );
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Failed to send Telegram photo: {ex.Message}");
                throw;
            }
        }
    }
}
