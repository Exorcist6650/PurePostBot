using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using SqlDB;

namespace DataManagement
{
    public sealed record User(
        long Id,
        long? GroupId,
        string? Caption,
        bool IsChangingGroupId,
        bool IsChangingCaption);


    // Users table CRUD wrap
    public class UserRepository
    {
        private readonly SqlDb _db;

        public UserRepository(SqlDb db) => _db = db;

        public object NullableToDb(long? v) => v.HasValue ? v.Value : DBNull.Value;
        public object NullableToDb(string? v) => v is null ? DBNull.Value : v;

        // Create
        public Task<int> CreateAsync(
            long userId,
            long? groupId,
            string? caption,
            bool isChangingGroupId,
            bool isChangingCaption) =>

            _db.ExecuteAsync(
                @"INSERT INTO Users(Id, GroupId, Caption, IsChangingGroupId, IsChangingCaption) 
                VALUES(@Id, @GroupId, @Caption, @IsChangingGroupId, @IsChangingCaption)",
                new SqliteParameter("@Id", userId),
                new SqliteParameter("@GroupId", NullableToDb(groupId)),
                new SqliteParameter("@Caption", NullableToDb(caption)),
                new SqliteParameter("@IsChangingGroupId", isChangingGroupId),
                new SqliteParameter("@IsChangingCaption", isChangingCaption));

        public Task<int> CreateAsync(User user) =>
            _db.ExecuteAsync(
                @"INSERT INTO Users(Id, GroupId, Caption, IsChangingGroupId, IsChangingCaption) 
                VALUES(@Id, @GroupId, @Caption, @IsChangingGroupId, @IsChangingCaption)",
                new SqliteParameter("@Id", user.Id),
                new SqliteParameter("@GroupId", NullableToDb(user.GroupId)),
                new SqliteParameter("@Caption", user.Caption),
                new SqliteParameter("@IsChangingGroupId", user.IsChangingGroupId),
                new SqliteParameter("@IsChangingCaption", user.IsChangingCaption));

        // Read
        public Task<User?> GetByIdAsync(long userId) =>
            _db.QuerySingleAsync(
                "SELECT Id, GroupId, Caption, IsChangingGroupId, IsChangingCaption FROM Users WHERE Id = @Id",
                reader => 
                {
                    long uId = reader.GetInt64(0);
                    long? uGroupId = !reader.IsDBNull(1) ? reader.GetInt64(1) : null;
                    string? uCaption = !reader.IsDBNull(2) ? reader.GetString(2) : null;
                    bool uIsChangingGroupId = reader.GetBoolean(3);
                    bool uIsChangingCaption = reader.GetBoolean(4);

                    return new User(uId, uGroupId, uCaption, uIsChangingGroupId, uIsChangingCaption); 
                },
                new SqliteParameter("@Id", userId));

        // Read all
        public Task<List<User>> GetAllAsync() =>
            _db.QueryAsync(
                "SELECT Id, GroupId, Caption, IsChangingGroupId, IsChangingCaption FROM Users ORDER BY Id",
                 reader =>
                 {
                     long uId = reader.GetInt64(0);
                     long? uGroupId = !reader.IsDBNull(1) ? reader.GetInt64(1) : null;
                     string? uCaption = !reader.IsDBNull(2) ? reader.GetString(2) : null;
                     bool uIsChangingGroupId = reader.GetBoolean(3);
                     bool uIsChangingCaption = reader.GetBoolean(4);

                     return new User(uId, uGroupId, uCaption, uIsChangingGroupId, uIsChangingCaption);
                 });

        // Update
        public Task<int> UpdateAsync(
            long userId,
            long? groupId,
            string? caption,
            bool isChangingGroupId,
            bool isChangingCaption) =>

            _db.ExecuteAsync(
                @"UPDATE Users SET 
                GroupId=@GroupId, 
                Caption=@Caption, 
                IsChangingGroupId=@IsChangingGroupId, 
                IsChangingCaption=@IsChangingCaption 
                WHERE Id=@Id",
                new SqliteParameter("@Id", userId),
                new SqliteParameter("@GroupId", NullableToDb(groupId)),
                new SqliteParameter("@Caption", NullableToDb(caption)),
                new SqliteParameter("@IsChangingGroupId", isChangingGroupId),
                new SqliteParameter("@IsChangingCaption", isChangingCaption));

        public Task<int> UpdateAsync(User user) =>

            _db.ExecuteAsync(
                @"UPDATE Users SET 
                GroupId=@GroupId, 
                Caption=@Caption, 
                IsChangingGroupId=@IsChangingGroupId, 
                IsChangingCaption=@IsChangingCaption 
                WHERE Id=@Id",
                new SqliteParameter("@Id", user.Id),
                new SqliteParameter("@GroupId", NullableToDb(user.GroupId)),
                new SqliteParameter("@Caption", NullableToDb(user.Caption)),
                new SqliteParameter("@IsChangingGroupId", user.IsChangingGroupId),
                new SqliteParameter("@IsChangingCaption", user.IsChangingCaption));

        // Delete
        public Task<int> DeleteAsync(long userId) =>
            _db.ExecuteAsync(
                "DELETE FROM Users WHERE Id=@Id",
                new SqliteParameter("@Id", userId));

    }
}
