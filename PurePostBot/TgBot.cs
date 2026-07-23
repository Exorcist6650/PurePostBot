using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PurePostBot
{
    public enum MessageType : byte
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

    class TgBot
    {
        // Dependencies

        private Host _host;


        // Fields

        private ConcurrentDictionary<string, List<Message>?> MediaGroupBuffer = new();

        public TgBot(Host host)
        {
            _host = host;
        }

        public async Task Init()
        {
            await _host.Start(); // Bot starting

            // Events binding
            _host.OnMessage += OnMessage;
        }

        // Events
        private async void OnMessage(ITelegramBotClient client, Update update)
        {
            // Validation and initialization
            if (update?.Message is not { } message) return; // Message
            if (message.Chat?.Id is not { } chatId) return; // ChatId

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

                // Initializing list for media group id
                if (!MediaGroupBuffer.TryGetValue(groupId, out var list) || list == null)
                {
                    list = new List<Message>();
                    MediaGroupBuffer[groupId] = list;
                }

                list.Add(message); // Addding message to the buffer

            }

            // Send message to user
            await SafeMode(() => SendMessage(client, chatId, message, messageType));
            await Task.CompletedTask;
        }

        // Methods

        public async Task SendMessage(ITelegramBotClient client, ChatId chatId,
            Message message, MessageType? type)
        {
            if (type is null) return;

            switch (type)
            {
                case MessageType.Animation:
                    await client.SendAnimation(chatId, message.Animation, caption: message.Caption);
                    break;

                case MessageType.Audio:
                    await client.SendAudio(chatId, message.Audio, caption: message.Caption);
                    break;

                case MessageType.Document:
                    await client.SendDocument(chatId, message.Document, caption: message.Caption);
                    break;

                case MessageType.Message:
                    await client.SendMessage(chatId, message.Text);
                    break;

                case MessageType.Photo:
                    await client.SendPhoto(chatId, message.Photo[^1], caption: message.Caption);
                    break;

                case MessageType.Sticker:
                    await client.SendSticker(chatId, message.Sticker);
                    break;

                case MessageType.Video:
                    await client.SendVideo(chatId, message.Video, caption: message.Caption);
                    break;

                case MessageType.Voice:
                    await client.SendVoice(chatId, message.Voice, caption: message.Caption);
                    break;
            }
        }

        public async Task SendMediaGroup(ITelegramBotClient client, ChatId chatId, Message message)
        {
            // Waithing for all messages downloading
            await Task.Delay(1000);

            // Get media group from buffer and remove key
            var mediaGroup = GetMediaGroup(message.MediaGroupId);

            if (mediaGroup != null && mediaGroup.Count > 0)
            {
                // Sending media group
                await SafeMode(() => client.SendMediaGroup(chatId, mediaGroup));
            }
        }

        private List<IAlbumInputMedia>? GetMediaGroup(string mediaGroupId)
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
                            case MessageType.Photo:
                                mediaGroup.Add(new InputMediaPhoto(message.Photo[^1]) { Caption = message.Caption });
                                break;

                            case MessageType.Video:
                                mediaGroup.Add(new InputMediaVideo(message.Video) { Caption = message.Caption });
                                break;

                            case MessageType.Document:
                                mediaGroup.Add(new InputMediaDocument(message.Document) { Caption = message.Caption });
                                break;

                            case MessageType.Audio:
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

        private MessageType? GetMessageType(Message message)
        {
            if (message.Text != null) return MessageType.Message;
            else if (message.Photo != null && message.Photo.Length > 0) return MessageType.Photo;
            else if (message.Video != null) return MessageType.Video;
            else if (message.Animation != null) return MessageType.Animation;
            else if (message.Audio != null) return MessageType.Audio;
            else if (message.Voice != null) return MessageType.Voice;
            else if (message.Document != null) return MessageType.Document;
            else if (message.Sticker != null) return MessageType.Sticker;
            else return null;
        }

        // Tools 

        /// <summary>
        /// Used to safe sending and deleting messages
        /// </summary>
        /// <param name="action">Send or delete telegram method</param>
        /// <returns></returns>
        public async Task SafeMode(Func<Task> action)
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
