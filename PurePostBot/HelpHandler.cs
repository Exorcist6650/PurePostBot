using Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot;

namespace PurePostBot
{
    public class HelpHandler
    {
        private readonly ITelegramBotClient _bot;

        public HelpHandler(ITelegramBotClient bot) => _bot = bot;

        public async Task HandleAsync(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;

            // Greeting text
            var text = RepliesReadService.GetReply("help_text");

            // Sending
            await PostingService.Send(_bot, chatId, new Message() { Text = text });
        }
    }
}
