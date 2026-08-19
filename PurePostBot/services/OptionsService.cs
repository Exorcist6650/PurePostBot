using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DataManagement;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Services
{
    public class OptionsService(ITelegramBotClient bot, UserService userService)
    {
        private readonly ITelegramBotClient _bot = bot;
        private readonly UserService _userService = userService;


        // GENERAL

        public async Task InterruptAllProcessesAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Read data
            if (await _userService.GetUserAsync(userId) is not { } user) return;

            var updateUser = user with 
            { 
                IsChangingGroupId = false,
                IsChangingCaption = false,
            };

            // Set default status
            await _userService.UpdateAsync(updateUser);

            // Delete options message
            await PostingService.Remove(_bot, userId, query.Message);
        }


        // CHANGE GROUP ID METHODS

        public async Task StartChangeGroupIdProcessAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Read data
            if (await _userService.GetUserAsync(userId) is not { } user) return;

            var updateUser = user with { IsChangingGroupId = true };

            // Set status
            await _userService.UpdateAsync(updateUser);

            // Send message
            await PostingService.Send(_bot, userId, new Message
            { Text = RepliesReadService.GetReply("set_group") });
        }

        public async Task RemoveGroupIdProcessAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Read data
            if (await _userService.GetUserAsync(userId) is not { } user) return;

            var updateUser = user with
            {
                GroupId = null,
                IsChangingGroupId = false,
                IsChangingCaption = false
            };

            // Clear group id
            await _userService.UpdateAsync(updateUser);

            // Send message
            await PostingService.Send(_bot, userId, new Message
            { Text = RepliesReadService.GetReply("remove_group") });
        }


        public async Task ChangeUserGroupIdAsync(long userId, long? groupId)
        {
            // Read data
            if (await _userService.GetUserAsync(userId) is not { } user) return;

            var updateUser = user with 
            { 
                GroupId = groupId,
                IsChangingGroupId = false 
            };

            // Update data
            await _userService.UpdateAsync(updateUser);
        }

        
        // CAPTION METHODS

        public async Task StartChangeCaptionProcessAsync(CallbackQuery query)
        {
            if (query?.Message?.Chat.Id is not { } userId) return;

            // Read data
            if (await _userService.GetUserAsync(userId) is not { } user) return;

            var updateUser = user with
            {
                IsChangingCaption = true
            };

            // Update status
            await _userService.UpdateAsync(updateUser);

            // Send message
            await PostingService.Send(_bot, userId, new Message
            { Text = RepliesReadService.GetReply("set_caption") });
        }

        public async Task ChangeCaptionAsync(long userId, string caption)
        {
            // Read data
            if (await _userService.GetUserAsync(userId) is not { } user) return;

            var updateUser = user with
            {
                Caption = caption,
                IsChangingCaption = false
            };

            // Update data
            await _userService.UpdateAsync(updateUser);
        }
    }
}