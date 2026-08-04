using System.Data.SqlTypes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Utils;

namespace Services
{
    public class GroupIdService
    {
        private readonly ITelegramBotClient _bot;
        private readonly UserService _userService;
        private readonly OptionsService _optionsService;

        public GroupIdService(ITelegramBotClient bot, UserService userService, OptionsService optionsService)
        {
            _bot = bot;
            _userService = userService;
            _optionsService = optionsService;
        }

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
                    return true;
                }
                else
                {
                    // Send faliled message
                    await PostingService.Send(_bot, userId, new Message
                        { Text = RepliesReadService.GetReply("set_group_failed") });
                    return false;
                }
            }
            else throw new SqlNullValueException("User is not register");
        }

        public async Task<bool> TrySetGroupId(Message message, long userId)
        {
            // Try get group
            if (await GetGroupId(message) is { } groupId)
            {
                await _optionsService.ChangeUserGroupAsync(userId, groupId); // Change user data 
                return true;
            }

            return false; // Bot is not invited and message is not from group | channel
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
