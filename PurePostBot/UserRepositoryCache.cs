using System.Data.SqlTypes;
using Microsoft.Extensions.Caching.Memory;
using Utils;



namespace DataManagement
{

    public class UserRepositoryCache
    {
        private readonly UserRepository _repo;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _ttl;

        public UserRepositoryCache(UserRepository repo, IMemoryCache cache, TimeSpan ttl)
        {
            _repo = repo;
            _cache = cache;
            _ttl = ttl;
        }

        // Key
        private static string Key(long userId) => $"user:{userId}";

        // Get from chache or add
        public Task<User> GetAsync(long userId)
        {
            return _cache.GetOrCreateAsync(Key(userId), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _ttl;

                if (await _repo.GetByIdAsync(userId) is not { } user) throw new SqlNullValueException("User is not register");
                return user;
            })!;
        }

        // Update db and remove cache
        public async Task UpdateAsync(
            long userId,
            long? groupId,
            string? caption,
            bool isChangingGroupId,
            bool isChangingCaption)
        {
            await _repo.UpdateAsync(userId, groupId, caption, isChangingGroupId, isChangingCaption);
            _cache.Remove(Key(userId));
        }

        public async Task UpdateAsync(User user)
        {
            await _repo.UpdateAsync(user);
            _cache.Remove(Key(user.Id));
        }

        public void RemoveAsync(long id)
        {
            _cache.Remove(Key(id));
            
        }
    }
}
