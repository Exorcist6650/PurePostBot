using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types;

namespace PurePostBot.services
{
    public class PostingCacheService(IMemoryCache cache, TimeSpan ttl)
    {
        private readonly IMemoryCache _cache = cache;
        private readonly TimeSpan _ttl = ttl;

        private static string Key(long chatId, long messageId) => $"{chatId}:{messageId}";

        public string Set(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return string.Empty;
            if (message?.Id is not { } messageId) return string.Empty;
            
            var key = Key(chatId, messageId);
            _cache.Set(key, message, _ttl);// Append message to cache

            return key;
        }

        public string Set(ConcurrentQueue<Message> messages)
        {
            messages.TryPeek(out var first);

            if (first?.Chat.Id is not { } chatId) return string.Empty;
            if (first?.Id is not { } messageId) return string.Empty;

            var key = Key(chatId, messageId);
            _cache.Set(key, messages, _ttl); // Append message to cache

            return key;
        }

        public T? Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var v) && v is T typed) return typed;
            return default;
        }

        public void Remove(string key) =>
            _cache.Remove(key);
    }
}
