using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Utils;

namespace TgBot
{
    class Host
    {
        // Events
        public Action<ITelegramBotClient, Update>? OnMessage;

        // Fields
        public User Me { get; private set; } // Bot info

        public TelegramBotClient _telegramBot { get; private set; } // Instance


        // Constructor
        public Host(string token)
        {
            _telegramBot = new TelegramBotClient(token);
        }

        public async Task Start()
        {
            Me = await _telegramBot.GetMe(); // Get bot info
            _telegramBot.StartReceiving(updateHandler, ErrorHandler); // Start

            ConsoleLogger.Log("Start receiving"); // Log
        }

        // Handlers
        private async Task updateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            OnMessage?.Invoke(client, update); // Event calling

            ConsoleLogger.Log(update?.Message?.Text ?? "Nothing"); // Log
            await Task.CompletedTask;
        }

        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            ConsoleLogger.Log(exception.Message, LogStatus.Error); // Log
            await Task.CompletedTask;
        }

    }
}
