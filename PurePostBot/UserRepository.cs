using Microsoft.Data.SqlClient;
using SqlDB;

namespace DataManagement
{
    public sealed record User(string Id, string? GroupId);

    // Users table CRUD wrap
    public static class UserRepository
    {
        // Create
        public static Task<int> CreateAsync(SqlDb db, string id, string groupId) =>
            db.ExecuteAsync(
                "INSERT INTO Users(Id, GroupId) VALUES(@Id, @GroupId)",
                new SqlParameter("@Id", id),
                new SqlParameter("@GroupId", groupId));

        // Read
        public static Task<User?> GetByIdAsync(SqlDb db, string id) =>
            db.QuerySingleAsync(
                "SELECT Id, GroupId FROM Users WHERE Id = @Id",
                reader => 
                {
                    string uId = reader.GetString(0);
                    string? uGroupId = reader.IsDBNull(1) ? null : reader.GetString(1);
                    return new User(uId, uGroupId); 
                },
                new SqlParameter("@Id", id));

        // Read all
        public static Task<List<User>> GetAllAsync(SqlDb db) =>
            db.QueryAsync(
                "SELECT Id, GroupId FROM Users ORDER BY Id",
                 reader =>
                 {
                     string uId = reader.GetString(0);
                     string? uGroupId = reader.IsDBNull(1) ? null : reader.GetString(1);
                     return new User(uId, uGroupId);
                 });

        // Update
        public static Task<int> UpdateGroupIdAsync(SqlDb db, string id, string? groupId) =>
            db.ExecuteAsync(
                "UPDATE Users SET GroupId=@GroupId WHERE Id=@Id",
                new SqlParameter("@Id", id),
                new SqlParameter("@GroupId", groupId));

        // Delete
        public static Task<int> DeleteAsync(SqlDb db, string id) =>
            db.ExecuteAsync(
                "DELETE FROM Users WHERE Id=@Id",
                new SqlParameter("@Id", id));

    }
}
