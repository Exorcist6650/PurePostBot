using Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot;

namespace Handlers
{
    public class StartHandler
    {
        private readonly ITelegramBotClient _bot;

        public StartHandler(ITelegramBotClient bot) => _bot = bot;

        public async Task HandleAsync(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;

            // Greeting text
            var text = RepliesReadService.GetReply("start_text");

            // Sending
            await PostingService.Send(_bot, chatId, new Message() { Text = text });
        }
    }
}
