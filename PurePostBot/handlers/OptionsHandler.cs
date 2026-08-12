using Telegram.Bot;
using SqlDB;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Services;
using Utils;

namespace Handlers
{
    public class OptionsHandler : IMessageHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly UserService _userService;

        private readonly InlineKeyboardMarkup _inlineKeyboard;

        public OptionsHandler(ITelegramBotClient bot, UserService userService)
        {
            _bot = bot;
            _userService = userService;

            // Buttons
            _inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    // CHANGE GROUP button 
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("options_change_group"), "action:options_change_group"),

                    // REMOVE GROUP button 
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("options_remove_group"), "action:options_remove_group"),
                },
                new[]
                {
                    // CANCEL button
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("cancel"), "action:options_cancel")
                }
            });
            _userService = userService;
        }



        public async Task HandleAsync(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;

            var user = await _userService.GetUserAsync(chatId);
            string text = $"{message.Chat.FirstName} {message.Chat.LastName} \nGroupId: {user?.GroupId}";

            await PostingService.Send(_bot, chatId, new Message() { Text = text, ReplyMarkup = _inlineKeyboard});
        }
    }
}
