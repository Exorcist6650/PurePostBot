using Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot;

namespace PurePostBot
{
    public class MessageHandler
    {
        private readonly ITelegramBotClient _bot;

        public MessageHandler(ITelegramBotClient bot) => _bot = bot;

        public async Task HandleAsync(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;

            
        }

        
    }
}
