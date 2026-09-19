using Telegram.Bot;
using Telegram.Bot.Types;

namespace PurePostBot.services
{
    public class OptionsService(ITelegramBotClient bot, UserService userService)
    {
        private readonly ITelegramBotClient _bot = bot;
        private readonly UserService _userService = userService;


        // GENERAL

        public async Task InterruptAllProcessesAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Remove flags
            await _userService.UpdateAsync(userId, user =>
            {
                user.IsChangingGroupId = false;
                user.IsChangingCaption = false;
            });

            // Delete options message
            await PostingService.Remove(_bot, userId, query.Message);
        }


        // CHANGE GROUP ID METHODS

        public async Task StartChangeGroupIdProcessAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Set status
            await _userService.UpdateAsync(userId, user =>
            {
                user.IsChangingGroupId = true;
            });

            // Send message
            await PostingService.Send(_bot, userId, new Message
            { Text = RepliesReadService.GetReply("set_group") });
        }

        public async Task RemoveGroupIdProcessAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Update data and reset flags
            await _userService.UpdateAsync(userId, user =>
            {
                user.GroupId = null;
                user.IsChangingGroupId = false;
                user.IsChangingCaption = false;
            });

            // Send message
            await PostingService.Send(_bot, userId, new Message
            { Text = RepliesReadService.GetReply("remove_group") });
        }

        public async Task ChangeUserGroupIdAsync(long userId, long groupId)
        {
            // Update data and reset flags
            await _userService.UpdateAsync(userId, user =>
            {
                user.GroupId = groupId;
                user.IsChangingGroupId = false;
                user.IsChangingCaption = false;
            });
        }


        // CAPTION METHODS

        public async Task StartChangeCaptionProcessAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Update status
            await _userService.UpdateAsync(userId, user =>
            {
                user.IsChangingCaption = true;
            });

            // Send message
            await PostingService.Send(_bot, userId, new Message
            { Text = RepliesReadService.GetReply("set_caption") });
        }

        public async Task ChangeCaptionAsync(long userId, string caption)
        {
            // Update data and reset flags
            await _userService.UpdateAsync(userId, user =>
            {
                user.Caption = caption;
                user.IsChangingGroupId = false;
                user.IsChangingCaption = false;
            });
        }
    }
}