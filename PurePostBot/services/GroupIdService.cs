using System.Data.SqlTypes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Utils;

namespace Services
{
    public class GroupIdService(ITelegramBotClient bot, UserService userService, OptionsService optionsService)
    {
        private readonly ITelegramBotClient _bot = bot;
        private readonly UserService _userService = userService;
        private readonly OptionsService _optionsService = optionsService;

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="userId"></param>
        /// <returns>True if user is changing group id, false otherwise</returns>
        public async Task<bool> HandleChangingGroupIdAsync(Message message, long userId)
        {
            if ((await _userService.GetUserAsync(userId)).IsChangingGroupId)
            {
                // Try get group
                if (await TrySetGroupId(message, userId))
                {
                    // Send success message
                    await PostingService.Send(_bot, userId, new Message
                    { Text = RepliesReadService.GetReply("set_group_success") });
                }
                else
                {
                    // Send faliled message
                    await PostingService.Send(_bot, userId, new Message
                    { Text = RepliesReadService.GetReply("set_group_failed") });
                }

                return true;
            }
            return false;
        }

        public async Task<bool> TrySetGroupId(Message message, long userId)
        {
            // Try get group
            if (await GetGroupId(message) is { } groupId)
            {
                await _optionsService.ChangeUserGroupIdAsync(userId, groupId); // Change user data 
                return true;
            }

            return false; // Bot is not invited and message is not from group | channel
        }

        public static async Task<bool> IsGroupMember(ITelegramBotClient bot, long groupId)
        {
            try
            {
                // Get member
                var member = await bot.GetChatMember(groupId, (await bot.GetMe()).Id);

                if (member != null && member.IsAdmin)
                    return true; // Is member
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log(ex.Message, ELogStatus.Error); // Log
            }

            return false; // Is not
        }

        private async Task<long?> GetGroupId(Message message)
        {
            if (message.ForwardFromChat != null)
            {
                // Checking group status
                if (message.ForwardFromChat.Type == Telegram.Bot.Types.Enums.ChatType.Channel
                    || message.ForwardFromChat.Type == Telegram.Bot.Types.Enums.ChatType.Group
                    || message.ForwardFromChat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
                {
                    try
                    {
                        // Get member
                        var member = await _bot.GetChatMember(message.ForwardFromChat, (await _bot.GetMe()).Id);

                        if (member != null && member.IsAdmin)
                        {
                            return message.ForwardFromChat.Id; // Return group id
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogger.Log(ex.Message, ELogStatus.Error); // Log
                    }
                }
            }

            return null;
        }
    }
}
