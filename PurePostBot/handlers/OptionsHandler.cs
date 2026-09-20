using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using PurePostBot.services;
using PurePostBot.utils;

namespace PurePostBot.handlers
{
    public class OptionsHandler(ITelegramBotClient bot, UserService userService) : IMessageHandler
    {
        private readonly ITelegramBotClient _bot = bot;
        private readonly UserService _userService = userService;

        private readonly InlineKeyboardMarkup _inlineKeyboard = new(
            [
                [
                    // CHANGE GROUP button 
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("options_change_group"), "action:options_change_group"),

                    // REMOVE GROUP button 
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("options_remove_group"), "action:options_remove_group"),
                ],
                [
                    // SET CAPTION button
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("options_set_caption"), "action:options_set_caption")
                ],
                [
                    // CANCEL button
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("cancel"), "action:options_cancel")
                ]
            ]);

        public async Task HandleAsync(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;
            
            var user = await _userService.GetUserAsync(chatId);
            string text = $"{message.Chat.FirstName} {message.Chat.LastName} \n" +
                          $"GroupId: {Convert.ToString(user?.GroupId) ?? "None"}\n" +
                          $"Caption: {Convert.ToString(user?.Caption) ?? "None"}\n";

            await PostingService.Send(_bot, chatId, new Message() { Text = text, ReplyMarkup = _inlineKeyboard });
        }
    }
}
