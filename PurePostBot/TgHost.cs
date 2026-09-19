using PurePostBot.utils;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PurePostBot
{
    class TgHost(ITelegramBotClient telegramBotClient)
    {
        // Events
        public Action<ITelegramBotClient, Update>? OnMessage;
        public Action<ITelegramBotClient, CallbackQuery>? OnCallback;

        // Fields
        public User Me { get; private set; } = null!; // Bot info

        public readonly ITelegramBotClient TelegramBot = telegramBotClient;

        public async Task Start()
        {
            Me = await TelegramBot.GetMe(); // Get bot info
            TelegramBot.StartReceiving(UpdateHandler, ErrorHandler); // Start

            ConsoleLogger.Log("Start receiving"); // Log
        }

        // Handlers
        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            // Button callback
            if (update.Type == UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery is { } callback)
                {
                    OnCallback?.Invoke(client, callback); // Event calling

                    ConsoleLogger.Log($"Button {callback.Data}"); // Log
                }
            }
            // Message
            else 
            {
                OnMessage?.Invoke(client, update); // Event calling

                ConsoleLogger.Log(update?.Message?.Text ?? "Nothing"); // Log
            }

            await Task.CompletedTask;
        }

        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            ConsoleLogger.Log(exception.Message, ELogStatus.Error); // Log
            await Task.CompletedTask;
        }

    }
}
