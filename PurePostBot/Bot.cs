using Telegram.Bot;
using Telegram.Bot.Types;
using Utils;
using Services;
using Handlers;

namespace TgBot
{
    class Bot
    {
        // Dependencies
        private readonly Host _host;
        private readonly StartHandler _startHandler;
        private readonly HelpHandler _helpHandler;
        private readonly OptionsHandler _optionsHandler;
        private readonly MessageHandler _messageHandler;
        private readonly UserService _userService;
        private readonly OptionsService _optionsService;
        private readonly GroupIdService _groupIdService;

        public Bot(Host host, UserService userService, MediaGroupService mediaGroupService)
        {
            _host = host;
            _startHandler = new StartHandler(_host._telegramBot, userService);
            _helpHandler = new HelpHandler(_host._telegramBot);
            _optionsHandler = new OptionsHandler(_host._telegramBot, userService);
            _messageHandler = new MessageHandler(_host._telegramBot, mediaGroupService, userService);
            _userService = userService;
            _optionsService = new OptionsService(_host._telegramBot, userService);
            _groupIdService = new GroupIdService(_host._telegramBot, userService, _optionsService);
        }

        public async Task Init()
        {
            await _host.Start(); // Bot starting

            // Events binding
            _host.OnMessage += OnMessage;
            _host.OnCallback += OnCallback;
        }


        // Delegates
        private async void OnMessage(ITelegramBotClient client, Update update)
        {
            // Validation and initialization
            if (update?.Message is not { } message) return; // Message
            if (message.Chat?.Id is not { } chatId) return; // ChatId

            // Checking, execute commands and return if message is a command
            if (await DispatchCommandAsync(message)) return;

            // Checking status, set groupid and return if status is true
            if (!await _groupIdService.HandleChangingGroupIdAsync(message, chatId)) return;
            

            // Otherwise handle message
            await _messageHandler.HandleAsync(message);
        }

        private async void OnCallback(ITelegramBotClient client, CallbackQuery query)
        {
            switch (query.Data)
            {
                case "action:cancel":
                    await PostingService.Remove(client, query.Message.Chat.Id, query.Message);
                    break;

                case "action:change_group":
                    await _optionsService.StartChangeGroupProcess(query);
                    break;

                default:
                    break;
            }
        }


        // Methods
        public async Task<bool> DispatchCommandAsync(Message message)
        {
            if (message.Text is not string text) return false;

            if (text.StartsWith("/start"))
                await _startHandler.HandleAsync(message);

            else if (text.StartsWith("/help"))
                await _helpHandler.HandleAsync(message);

            else if (text.StartsWith("/options"))
                await _optionsHandler.HandleAsync(message);

            else
                return false;

            return true; // If command was execute
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

        public async Task<long?> GetGroupId(Message message)
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
