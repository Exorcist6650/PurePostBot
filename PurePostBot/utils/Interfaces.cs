using Telegram.Bot.Types;

namespace PurePostBot.utils
{
    interface IMessageHandler
    {
        public Task HandleAsync(Message message);
    }
}
