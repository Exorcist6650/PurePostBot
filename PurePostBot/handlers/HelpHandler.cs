using Telegram.Bot;
using Telegram.Bot.Types;
using PurePostBot.services;
using PurePostBot.utils;

namespace PurePostBot.handlers
{
    public class HelpHandler(ITelegramBotClient bot) : IMessageHandler
    {
        private readonly ITelegramBotClient _bot = bot;

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
