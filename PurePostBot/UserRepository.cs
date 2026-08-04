using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using SqlDB;

namespace DataManagement
{
    public sealed record User(long Id, long? GroupId, bool IsChangingGroupId);

    // Users table CRUD wrap
    public class UserRepository
    {
        private readonly SqlDb _db;

        public UserRepository(SqlDb db) => _db = db;

        public object NullableToDb(long? v) => v.HasValue ? v.Value : DBNull.Value;

        // Create
        public Task<int> CreateAsync(long userId, long? groupId, bool isChangingGroupId) =>
            _db.ExecuteAsync(
                "INSERT INTO Users(Id, GroupId, IsChangingGroupId) VALUES(@Id, @GroupId, @IsChangingGroupId)",
                new SqliteParameter("@Id", userId),
                new SqliteParameter("@GroupId", NullableToDb(groupId)),
                new SqliteParameter("@IsChangingGroupId", isChangingGroupId));

        public Task<int> CreateAsync(User user) =>
            _db.ExecuteAsync(
                "INSERT INTO Users(Id, GroupId, IsChangingGroupId) VALUES(@Id, @GroupId, @IsChangingGroupId)",
                new SqliteParameter("@Id", user.Id),
                new SqliteParameter("@GroupId", NullableToDb(user.GroupId)),
                new SqliteParameter("@IsChangingGroupId", user.IsChangingGroupId));

        // Read
        public Task<User?> GetByIdAsync(long userId) =>
            _db.QuerySingleAsync(
                "SELECT Id, GroupId, IsChangingGroupId FROM Users WHERE Id = @Id",
                reader => 
                {
                    long uId = reader.GetInt64(0);
                    long? uGroupId = !reader.IsDBNull(1) ? reader.GetInt64(1) : null;
                    bool uIsChangingGroupId = reader.GetBoolean(2);

                    return new User(uId, uGroupId, uIsChangingGroupId); 
                },
                new SqliteParameter("@Id", userId));

        // Read all
        public Task<List<User>> GetAllAsync() =>
            _db.QueryAsync(
                "SELECT Id, GroupId, IsChangingGroupId FROM Users ORDER BY Id",
                 reader =>
                 {
                     long uId = reader.GetInt64(0);
                     long? uGroupId = !reader.IsDBNull(1) ? reader.GetInt64(1) : null;
                     bool uIsChangingGroupId = reader.GetBoolean(2);

                     return new User(uId, uGroupId, uIsChangingGroupId);
                 });

        // Update
        public Task<int> UpdateAsync(long userId, long? groupId, bool IsChangingGroupId) =>
            _db.ExecuteAsync(
                "UPDATE Users SET GroupId=@GroupId, IsChangingGroupId=@IsChangingGroupId WHERE Id=@Id",
                new SqliteParameter("@Id", userId),
                new SqliteParameter("@GroupId", NullableToDb(groupId)), 
                new SqliteParameter("@IsChangingGroupId", IsChangingGroupId));

        // Delete
        public Task<int> DeleteAsync(long userId) =>
            _db.ExecuteAsync(
                "DELETE FROM Users WHERE Id=@Id",
                new SqliteParameter("@Id", userId));

    }
}
