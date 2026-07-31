using Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PurePostBot
{
    public class MessageHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly MediaGroupService _mediaGroupService;

        public MessageHandler(ITelegramBotClient bot, MediaGroupService mediaGroupService)
        {
            _bot = bot;
            _mediaGroupService = mediaGroupService;
        }

        public async Task HandleAsync(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;

            await PostingService.Remove(_bot, chatId, message); // Deleting 

            // Collect media group
            if (message.MediaGroupId != null)
            {
                _mediaGroupService.AppendToBuffer(message.MediaGroupId, message);

                await Task.Delay(1000);

                if (_mediaGroupService.GetMediaGroup(message.MediaGroupId) is { } mediaGroup)
                {
                    await PostingService.SendAlbum(_bot, chatId, mediaGroup); // Sending album to user
                }
            }
            else
                await PostingService.Send(_bot, chatId, message); // Sending message to user

        }
    }
}
