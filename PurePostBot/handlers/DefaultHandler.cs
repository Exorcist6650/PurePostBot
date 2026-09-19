using Telegram.Bot;
using Telegram.Bot.Types;
using PurePostBot.services;
using PurePostBot.utils;

namespace PurePostBot.handlers

{
    public class DefaultHandler(
        ITelegramBotClient bot,
        MediaGroupService mediaGroupService,
        PostEditService postEditService) : IMessageHandler
    {
        private readonly ITelegramBotClient _bot = bot;
        private readonly MediaGroupService _mediaGroupService = mediaGroupService;
        private readonly PostEditService _postEditService = postEditService;

        public async Task HandleAsync(Message message) =>
            await _postEditService.CreateEditMessage(message); // Create edit message

        public async Task HandleAlbumAsync(Message message)
        {
            if (message?.Chat.Id is not { }) return;
            if (message.MediaGroupId is not { } mediaGroupId) return;

            // Get all messages
            if (_mediaGroupService.GetMessages(mediaGroupId) is not { } messages) return;

            await _postEditService.CreateEditMessage(messages); // Create edit message
        }
    }
}
