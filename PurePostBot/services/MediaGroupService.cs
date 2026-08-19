using System.Collections.Concurrent;
using Telegram.Bot.Types;

namespace Services
{
    public class MediaGroupService
    {
        private ConcurrentDictionary<string, ConcurrentQueue<Message>> _mediaGroupBuffer = new();

        public void AppendToBuffer(string groupId, Message message)
        {
            var queue = _mediaGroupBuffer.GetOrAdd(groupId, new ConcurrentQueue<Message>());
            queue.Enqueue(message);
        }

        public ConcurrentQueue<Message>? GetMessages(string mediaGroupId)
        {
            // Getting all messages
            _mediaGroupBuffer.TryGetValue(mediaGroupId, out var messages);

            return (messages != null && messages.Count > 0) ? messages : null;
        }

        public bool TryRemoveFromBuffer(string mediaGroupId) =>
            _mediaGroupBuffer.TryRemove(mediaGroupId, out _);

        public List<IAlbumInputMedia>? GetRemoveMediaGroup(string mediaGroupId)
        {
            // Getting all messages and immediately removing the key
            if (_mediaGroupBuffer.TryRemove(mediaGroupId, out var messages))
            {
                if (messages != null && messages.Count > 0)
                {
                    return ConvertToMediaGroup(messages);
                }
            }
            return null;
        }

        public static List<IAlbumInputMedia> ConvertToMediaGroup(ConcurrentQueue<Message> messages)
        {
            var mediaGroup = new List<IAlbumInputMedia>();
            foreach (var message in messages)
            {
                // Get type
                var type = PostingService.GetMessageType(message);
                if (type == null) continue;

                // Adding media to the group
                switch (type)
                {
                    case EMessageType.Photo:
                        mediaGroup.Add(new InputMediaPhoto(message.Photo![^1]) { Caption = message.Caption });
                        break;

                    case EMessageType.Video:
                        mediaGroup.Add(new InputMediaVideo(message.Video!) { Caption = message.Caption });
                        break;

                    case EMessageType.Document:
                        mediaGroup.Add(new InputMediaDocument(message.Document!) { Caption = message.Caption });
                        break;

                    case EMessageType.Audio:
                        mediaGroup.Add(new InputMediaAudio(message.Audio!) { Caption = message.Caption });
                        break;

                    default:
                        continue; // Skip unsupported
                }
            }
            return mediaGroup;
        }
    }
}
