using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Utils;
using Services;
using Handlers;
using PurePostBot;

namespace TgBot
{
    public enum EMessageType : byte
    {
        Animation,
        Audio,
        Document,
        Message,
        Photo,
        Sticker,
        Video,
        Voice,
    }

    class Bot
    {
        // Dependencies
        private Host _host;
        private StartHandler _startHandler;
        private HelpHandler _helpHandler;


        // Fields
        private ConcurrentDictionary<string, ConcurrentQueue<Message>> MediaGroupBuffer = new();

        public Bot(Host host)
        {
            _host = host;
            _startHandler = new StartHandler(_host._telegramBot);
            _helpHandler = new HelpHandler(_host._telegramBot);
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

            // Delete user message
            await SafeMode(() => client.DeleteMessage(chatId, message.Id));

            // Message type validation 
            var messageType = GetMessageType(message);
            if (messageType == null)
            {
                ConsoleLogger.Log("Message type is unsupport!", LogStatus.Warning);
                return;
            }

            // Media group logic
            if (message.MediaGroupId != null)
            {
                string groupId = message.MediaGroupId;

                // Get queue for media group
                var queue = MediaGroupBuffer.GetOrAdd(groupId, _ => new ConcurrentQueue<Message>());

                queue.Enqueue(message); // Addding message to the buffer

                // Getting media group after waiting other messages
                await Task.Delay(1000);

                var mediaGroup = GetMediaGroup(message.MediaGroupId);
                if (mediaGroup != null)
                {
                    // Send media group to chat
                    await PostingService.SendAlbum(client, chatId, mediaGroup);
                }
            }
            // Standard logic
            else
            {
                // Send message to user
                await PostingService.Send(client, chatId, message, messageType);
            }
            await Task.CompletedTask;
        }

        // Load and packing media by id 
        public List<IAlbumInputMedia>? GetMediaGroup(string mediaGroupId)
        {
            // Getting all media and immediately removing the key
            if (MediaGroupBuffer.TryRemove(mediaGroupId, out var messages))
            {
                if (messages != null && messages.Count > 0)
                {
                    var mediaGroup = new List<IAlbumInputMedia>();
                    foreach (var message in messages)
                    {
                        // Get type
                        var type = GetMessageType(message);
                        if (type == null) continue;

                        // Adding media to the group
                        switch (type)
                        {
                            case EMessageType.Photo:
                                mediaGroup.Add(new InputMediaPhoto(message.Photo[^1]) { Caption = message.Caption });
                                break;

                            case EMessageType.Video:
                                mediaGroup.Add(new InputMediaVideo(message.Video) { Caption = message.Caption });
                                break;

                            case EMessageType.Document:
                                mediaGroup.Add(new InputMediaDocument(message.Document) { Caption = message.Caption });
                                break;

                            case EMessageType.Audio:
                                mediaGroup.Add(new InputMediaAudio(message.Audio) { Caption = message.Caption });
                                break;

                            default:
                                continue; // Skip unsupported
                        }
                    }
                    return mediaGroup;
                }
                return null;
            }
            return null;
        }

        public static EMessageType? GetMessageType(Message message)
        {
            if (message.Text != null) return EMessageType.Message;
            else if (message.Photo != null && message.Photo.Length > 0) return EMessageType.Photo;
            else if (message.Video != null) return EMessageType.Video;
            else if (message.Animation != null) return EMessageType.Animation;
            else if (message.Audio != null) return EMessageType.Audio;
            else if (message.Voice != null) return EMessageType.Voice;
            else if (message.Document != null) return EMessageType.Document;
            else if (message.Sticker != null) return EMessageType.Sticker;
            else return null;
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
                        ConsoleLogger.Log(ex.Message, LogStatus.Error); // Log
                    }
                }
            }
            return null;
        }

        // Tools 

        /// <summary>
        /// Used to safe sending and deleting messages
        /// </summary>
        /// <param name="action">Send or delete telegram method</param>
        /// <returns></returns>
        public static async Task SafeMode(Func<Task> action)
        {
            try
            {
                await action.Invoke(); // Invoke command
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log(ex.Message, LogStatus.Error); // Log
            }
        }
    }
}
