using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Services
{
    public class OptionsService
    {
        private readonly ITelegramBotClient _bot;
        private readonly UserService _userService;

        public OptionsService(ITelegramBotClient bot, UserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public async Task StartChangeGroupIdProcessAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Read data
            var user = await _userService.GetUserAsync(userId);

            // Set status
            await _userService.UpdateAsync(userId, user.GroupId, true);

            // Send message
            await PostingService.Send(_bot, userId, new Message 
                { Text = RepliesReadService.GetReply("set_group") });
        }

        public async Task RemoveGroupIdProcessAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Clear group id
            await _userService.UpdateAsync(userId, null, false);

            // Send message
            await PostingService.Send(_bot, userId, new Message
                { Text = RepliesReadService.GetReply("remove_group") });
        }

        public async Task InterruptChangeGroupIdAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Read data
            var user = await _userService.GetUserAsync(userId);

            // Set default status
            await _userService.UpdateAsync(userId, user.GroupId, false);

            // Delete options message
            await PostingService.Remove(_bot, userId, query.Message);
        }

        public async Task ChangeUserGroupIdAsync(long userId, long? groupId) =>
            // Update data and remove status
            await _userService.UpdateAsync(userId, groupId, false);

        public async Task RemoveUserGroupIdAsync(long userId) =>
            // Update data
            await _userService.UpdateAsync(userId, null, false);
    }
}