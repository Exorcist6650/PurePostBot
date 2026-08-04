using DataManagement;
using SqlDB;

namespace Services
{
    public class UserService
    {
        // Fields
        private readonly UserRepository _repo;
        private readonly UserRepositoryCache _cache;

        public UserService(UserRepository repo, UserRepositoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        /// <summary>
        /// Add a user to db. Uses only for new users.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <param name="isChangingGroupId"></param>
        /// <returns>
        /// true if successful false otherwise
        /// </returns>
        public async Task<bool> RegisterUserAsync(long userId, long? groupId, bool isChangingGroupId)
        {
            // If user is exist
            if (await _repo.GetByIdAsync(userId) is { }) return false;
            
            return await _repo.CreateAsync(userId, groupId, isChangingGroupId) > 0 ? true : false;
        }

        /// <summary>
        /// Get user from cache. Otherwise append user to db and return
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserAsync(long userId) => await _cache.GetAsync(userId);

        public async Task UpdateAsync(long userId, long? groupId, bool isChangingGroupId) =>
            await _cache.UpdateAsync(userId, groupId, isChangingGroupId);

        public async Task RemoveUserAsync(long userId)
        {
            await _repo.DeleteAsync(userId);
            _cache.RemoveAsync(userId);
        }

    }
}
