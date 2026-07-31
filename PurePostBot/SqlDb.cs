using System.Data;
using Microsoft.Data.SqlClient;

namespace SqlDB
{
    public sealed class DbOptions
    {
        public string ConnectionString { get; init; } = "";
    }
    public sealed class SqlDb
    {
        private readonly string _connectionString;

        public SqlDb(DbOptions options)
        {
            _connectionString = options.ConnectionString;
        }

        // Execute INSERT/UPDATE/DELETE
        public async Task<int> ExecuteAsync(string sqlQuery, params SqlParameter[] parameters)
        {
            // Set and open db connection
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Sql cmd
            await using var command = new SqlCommand(sqlQuery, connection); 
            if (parameters?.Length > 0) command.Parameters.AddRange(parameters);

            return await command.ExecuteNonQueryAsync(); // Return operations amount
        }

        // Execute SELECT one row
        public async Task<T?> QuerySingleAsync<T>(string sqlQuery, Func<IDataRecord, T> map, 
            params SqlParameter[] parameters)
        {
            // Set and open db connection
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Sql cmd
            await using var command = new SqlCommand(sqlQuery, connection);
            if (parameters?.Length > 0) command.Parameters.AddRange(parameters);

            // Read single row
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await reader.ReadAsync()) return default;

            return map(reader); // Return delegate result
        }

        // Execute SELECT list
        public async Task<List<T>> QueryAsync<T>(string sqlQuery, Func<IDataRecord, T> map,
            params SqlParameter[] parameters)
        {
            // Set and open db connection
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Sql cmd
            await using var command = new SqlCommand(sqlQuery, connection);
            if (parameters?.Length > 0) command.Parameters.AddRange(parameters);

            var result = new List<T>();

            // Read single row
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(map(reader));
            }

            return result; // Return list of delegate results
        }
    }
}
