using DataManagement;
using SqlDB;

namespace Services
{
    public class UserService
    {
        // Fields
        private readonly SqlDb _db;

        public UserService(SqlDb db) => _db = db;

        public async Task<int> RegisterUser(string id, string groupId) =>
            await UserRepository.CreateAsync(_db, id, groupId);

        public async Task<User?> GetUser(string id) =>
            await UserRepository.GetByIdAsync(_db, id);

        public async Task<int> ChangeGroupId(string id, string? groupId) =>
            await UserRepository.UpdateGroupIdAsync(_db, id, groupId);

        public async Task<int> RemoveUser(string id) =>
            await UserRepository.DeleteAsync(_db, id);

    }
}
