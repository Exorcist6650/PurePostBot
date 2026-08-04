using Services;
using SqlDB;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot;

namespace Handlers
{
    public class StartHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly UserService _userService;

        public StartHandler(ITelegramBotClient bot, UserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public async Task HandleAsync(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;

            // Greeting text
            var text = RepliesReadService.GetReply("start_text");

            // Sending
            if (await PostingService.Send(_bot, chatId, new Message() { Text = text }) is not { }) return;

            // Adding user to db
            await _userService.RegisterUserAsync(chatId, null, false);
            Console.WriteLine("all is good");
        }
    }
}
