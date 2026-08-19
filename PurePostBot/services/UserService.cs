using DataManagement;
using SqlDB;

namespace Services
{
    public class UserService(UserRepository repo, UserRepositoryCache cache)
    {
        // Fields
        private readonly UserRepository _repo = repo;
        private readonly UserRepositoryCache _cache = cache;

        /// <summary>
        /// Add a user to db. Uses only for new users.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <param name="isChangingGroupId"></param>
        /// <returns>
        /// true if successful false otherwise
        /// </returns>
        public async Task<bool> RegisterUserAsync(
            long userId,
            long? groupId,
            string? caption,
            bool isChangingGroupId,
            bool isChangingCaption)
        {
            // If user is exist
            if (await _repo.GetByIdAsync(userId) is { }) return false;
            
            return await _repo.CreateAsync(userId, groupId, caption, isChangingGroupId, isChangingCaption) > 0;
        }

        /// <summary>
        /// Get user from cache. Otherwise append user to db and return
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserAsync(long userId) => await _cache.GetAsync(userId);

        public async Task UpdateAsync(
            long userId,
            long? groupId,
            string? caption,
            bool isChangingGroupId,
            bool isChangingCaption) =>

            await _cache.UpdateAsync(userId, groupId, caption, isChangingGroupId, isChangingCaption);

        public async Task UpdateAsync(User user) =>

            await _cache.UpdateAsync(user);

        public async Task RemoveUserAsync(long userId)
        {
            await _repo.DeleteAsync(userId);
            _cache.RemoveAsync(userId);
        }

    }
}
