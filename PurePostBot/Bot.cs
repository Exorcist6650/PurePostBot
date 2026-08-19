using Telegram.Bot;
using Telegram.Bot.Types;
using Utils;
using Services;
using Handlers;
using System.Reflection.Emit;
using DataManagement;

namespace TgBot
{
    class Bot(TgHost host,
        UserService userService,
        MediaGroupService mediaGroupService,
        OptionsService optionsService,
        GroupIdService groupIdService,
        CaptionService captionService,
        PostingMessagesCache postingMessagesCache,
        PostEditService postEditService,

        StartHandler startHandler,
        HelpHandler helpHandler,
        OptionsHandler optionsHandler,
        DefaultHandler defaultHandler)
    {
        // Dependencies
        private readonly TgHost _host = host;
        private readonly StartHandler _startHandler = startHandler;
        private readonly HelpHandler _helpHandler = helpHandler;
        private readonly OptionsHandler _optionsHandler = optionsHandler;
        private readonly DefaultHandler _defaultHandler = defaultHandler;
        private readonly UserService _userService = userService;
        private readonly MediaGroupService _mediaGroupService = mediaGroupService;
        private readonly OptionsService _optionsService = optionsService;
        private readonly GroupIdService _groupIdService = groupIdService;
        private readonly CaptionService _captionService = captionService;
        private readonly PostingMessagesCache _postingMessagesCache = postingMessagesCache;
        private readonly PostEditService _postEditService = postEditService;

        public async Task InitAsync()
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

            // Case message is a part of album
            if (message.MediaGroupId is not null)
            {
                // Collect all messages to buffer
                _mediaGroupService.AppendToBuffer(message.MediaGroupId, message);

                // Waiting all messages from album
                await Task.Delay(1000);

                // Get first
                _mediaGroupService.GetMessages(message.MediaGroupId)!.TryPeek(out var firstMessage);

                // Handle only for first message
                if (ReferenceEquals(firstMessage, message))
                {
                    await HandleFlowAsync(message, _defaultHandler.HandleAlbumAsync); // Handle

                    _mediaGroupService.TryRemoveFromBuffer(message.MediaGroupId); // Remove group
                }
            }
            else
                // Default case
                await HandleFlowAsync(message, _defaultHandler.HandleAsync); // Handle
        }

        private async void OnCallback(ITelegramBotClient client, CallbackQuery query)
        {
            if (query.Message is not { } message) return;

            // Update button UI
            try
            {
                await client.AnswerCallbackQuery(query.Id);
            }
            catch(Exception ex)
            {
                ConsoleLogger.Log(ex.Message, ELogStatus.Warning);
            }

            // Handle buttons
            switch (query.Data)
            {
                case "action:cancel":
                    await PostingService.Remove(client, message.Chat.Id, message);
                    break;

                // Options message block
                case "action:options_change_group":
                    await _optionsService.StartChangeGroupIdProcessAsync(query);
                    break;

                case "action:options_remove_group":
                    await _optionsService.RemoveGroupIdProcessAsync(query);
                    break;

                case "action:options_set_caption":
                    await _optionsService.StartChangeCaptionProcessAsync(query);
                    break;

                case "action:options_cancel":
                    await _optionsService.InterruptAllProcessesAsync(query);
                    break;

                // Editing message block
                case "action:editing_send_post":
                    await _postEditService.SendToGroup(query);
                    break;

                case "action:editing_add_caption":
                    await _postEditService.AddCaption(query);
                    break;

                case "action:editing_cancel":
                    await _postEditService.CancelEditing(query);
                    break;
            }
        }


        // Methods
        public async Task HandleFlowAsync(Message message, Func<Message, Task> defaultHandler)
        {
            if (message.Chat?.Id is not { } chatId) return; // ChatId

            // Register a user if not
            await _userService.RegisterUserAsync(chatId, null, null, false, false);

            // Checking, execute commands and return if message is a command
            if (await DispatchCommandAsync(message)) return;

            // Checking status for set group id
            if (await _groupIdService.HandleChangingGroupIdAsync(message, chatId)) return;

            // Checking status for set caption
            if (await _captionService.HandleChangingCaptionAsync(message, chatId)) return;

            // Otherwise handle message
            await defaultHandler(message);
        }

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
                await _optionsService.ChangeUserGroupIdAsync(userId, groupId); // Change user data 
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
                        var member = await _host.TelegramBot.GetChatMember(message.ForwardFromChat, _host.Me.Id);

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
