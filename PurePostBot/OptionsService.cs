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

        public async Task StartChangeGroupProcess(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } chatId) return;

            // Read data
            var user = await _userService.GetUserAsync(chatId);

            // Set status
            await _userService.UpdateAsync(user.Id, user.GroupId, true);

            // Send message
            await PostingService.Send(_bot, chatId, new Message 
                { Text = RepliesReadService.GetReply("set_group") });
        }

        public async Task ChangeUserGroupAsync(long userId, long? groupId)
        {
            // Update data
            await _userService.UpdateAsync(userId, groupId, false);
        }
    }
}