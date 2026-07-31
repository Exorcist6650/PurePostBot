using Telegram.Bot;
using Telegram.Bot.Types;
using Utils;

namespace Services
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

    static class PostingService
    {
        // Post message to the chat
        public static async Task<Message?> Send(ITelegramBotClient client, ChatId chatId, Message message)
        {
            try
            {
                return await SendMessage(client, chatId, message, GetMessageType(message));
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log(ex.Message, ELogStatus.Error); // Log
            }
            return null;
        }

        // Post album to the chat
        public static async Task SendAlbum(ITelegramBotClient client, ChatId chatId, List<IAlbumInputMedia> mediaGroup)
        {
            try
            {
                await SendMediaGroup(client, chatId, mediaGroup);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log(ex.Message, ELogStatus.Error); // Log
            }
        }

        // Delete message 
        public static async Task Remove(ITelegramBotClient client, ChatId chatId, Message message)
        {
            try
            {
                await client.DeleteMessage(chatId, message.Id);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log(ex.Message, ELogStatus.Error); // Log
            }
        }

        // Message type
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

        // Send message
        private static async Task<Message?> SendMessage(ITelegramBotClient client, ChatId chatId,
            Message message, EMessageType? type)
        {
            if (type is null) return null;

            switch (type)
            {
                case EMessageType.Animation:
                    return await client.SendAnimation(chatId, message.Animation, caption: message.Caption);

                case EMessageType.Audio:
                    return await client.SendAudio(chatId, message.Audio, caption: message.Caption);

                case EMessageType.Document:
                    return await client.SendDocument(chatId, message.Document, caption: message.Caption);

                case EMessageType.Message:
                    return await client.SendMessage(chatId, message.Text);

                case EMessageType.Photo:
                    return await client.SendPhoto(chatId, message.Photo[^1], caption: message.Caption);

                case EMessageType.Sticker:
                    return await client.SendSticker(chatId, message.Sticker);

                case EMessageType.Video:
                    return await client.SendVideo(chatId, message.Video, caption: message.Caption);

                case EMessageType.Voice:
                    return await client.SendVoice(chatId, message.Voice, caption: message.Caption);
                default:
                    return null;
            }
        }

        // Send media group
        private static async Task SendMediaGroup(ITelegramBotClient client, ChatId chatId, List<IAlbumInputMedia> mediaGroup)
        {
            if (mediaGroup != null && mediaGroup.Count > 0)
            {
                // Sending media group
                await client.SendMediaGroup(chatId, mediaGroup);
            }
        }
    }
}
