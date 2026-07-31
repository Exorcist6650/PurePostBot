using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Utils;
using Services;
using Handlers;
using PurePostBot;

namespace TgBot
{
    class Bot
    {
        // Dependencies
        private Host _host;
        private StartHandler _startHandler;
        private HelpHandler _helpHandler;
        private MessageHandler _messageHandler;

        public Bot(Host host, MediaGroupService mediaGroupService)
        {
            _host = host;
            _startHandler = new StartHandler(_host._telegramBot);
            _helpHandler = new HelpHandler(_host._telegramBot);
            _messageHandler = new MessageHandler(_host._telegramBot, mediaGroupService);
        }

        public async Task Init()
        {
            await _host.Start(); // Bot starting

            // Events binding
            _host.OnMessage += OnMessage;
        }

        // Delegate
        private async void OnMessage(ITelegramBotClient client, Update update)
        {
            // Validation and initialization
            if (update?.Message is not { } message) return; // Message
            if (message.Chat?.Id is not { } chatId) return; // ChatId

            // Checking commands
            if (message?.Text != null)
            {
                if (message.Text.StartsWith("/start"))
                {
                    await _startHandler.HandleAsync(message);
                    return;
                }
                else if (message.Text.StartsWith("/help"))
                {
                    await _helpHandler.HandleAsync(message);
                    return;
                }

            }
            await _messageHandler.HandleAsync(message);
        }

        // Methods
        public async Task<ChatId?> GetGroupId(Message message)
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
                        var member = await _host._telegramBot.GetChatMember(message.ForwardFromChat, _host.Me.Id);

                        if (member != null && member.IsAdmin)
                        {
                            return message.ForwardFromChat; // Return group id
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
