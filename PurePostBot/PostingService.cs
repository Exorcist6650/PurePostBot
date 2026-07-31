using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot;
using Utils;

namespace Services
{
    static class PostingService
    {
        // Post message to the chat
        public static async Task<Message?> Send(ITelegramBotClient client, ChatId chatId, Message message, EMessageType? type)
        {
            try
            {
                return await SendMessage(client, chatId, message, type);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log(ex.Message, LogStatus.Error); // Log
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
                ConsoleLogger.Log(ex.Message, LogStatus.Error); // Log
            }
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
