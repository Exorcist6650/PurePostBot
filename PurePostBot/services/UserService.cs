using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PurePostBot.models;

namespace PurePostBot.services
{
    public class UserService(
        AppDbContext db,
        IMemoryCache cache,
        TimeSpan ttl)
    {
        // Fields
        private readonly AppDbContext _db = db;
        private readonly IMemoryCache _cache = cache;
        private readonly TimeSpan _ttl = ttl;


        // Key generator to cache
        private static string Key(long userId) => $"user:{userId}";


        // CRUD operations

        public async Task<UserClient> RegisterUserAsync(long userId)
        {
            var existingUser = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            // If user is exist return existing
            if (existingUser is not null) return existingUser;

            // Create user
            var user = new UserClient()
            {
                Id = userId,
                GroupId = null,
                Caption = null,
                IsChangingGroupId = false,
                IsChangingCaption = false
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.Entry(user).State = EntityState.Detached;

            // Return registered user
            return user;
        }

        public async Task<UserClient> GetUserAsync(long userId)
        {
            var user = await _cache.GetOrCreateAsync(
                Key(userId),
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = _ttl;

                    var user = await _db.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == userId);

                    return user ?? await RegisterUserAsync(userId);
                }
            );

            return user ?? throw new InvalidOperationException(
                    $"Can't load the user: {userId}");
        }

        public async Task UpdateAsync(
            long userId,
            Action<UserClient> update)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null) return; // If user is not exist

            update(user);

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            _cache.Remove(Key(userId)); // Clear cache
        }

        public async Task<bool> RemoveUserAsync(long userId)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null) return false; // If user is not exist

            // Clear
            _db.Users.Remove(user);
            _cache.Remove(Key(userId));

            await _db.SaveChangesAsync();

            return true;
        }

    }
}
