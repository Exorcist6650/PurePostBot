using Services;
using SqlDB;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot;
using Utils;

namespace Handlers
{
    public class StartHandler : IMessageHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly UserService _userService;

        private ReplyKeyboardMarkup _userButtonMenu;

        public StartHandler(ITelegramBotClient bot, UserService userService)
        {
            _bot = bot;
            _userService = userService;

            _userButtonMenu = new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton[]
                    {
                        RepliesReadService.GetButton("menu_create_post"),
                        RepliesReadService.GetButton("menu_create_button")
                    },

                    new KeyboardButton[]
                    {
                        RepliesReadService.GetButton("menu_settings")
                    }
                })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };
        }


        public async Task HandleAsync(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;

            // Greeting text
            var text = RepliesReadService.GetReply("start_text");

            // Sending message with buttons menu
            if (await PostingService.Send(_bot, chatId, new Message { Text = text}) is not { }) return;
           
            // Adding user to db
            await _userService.RegisterUserAsync(chatId, null, null, false, false);
        }
    }
}
