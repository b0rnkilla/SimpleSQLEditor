using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace EfPlayground.Services
{
    public class SqlServerAdminService
    {
        #region Methods

        public async Task<IReadOnlyList<string>> GetDatabasesAsync(string connectionString)
        {
            var masterConnectionString = BuildMasterConnectionString(connectionString);

            const string sql = @"
SELECT [name]
FROM sys.databases
WHERE [name] NOT IN ('master', 'model', 'msdb', 'tempdb')
ORDER BY [name];";

            var result = new List<string>();

            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }

            return result;
        }

        public async Task CreateDatabaseAsync(string connectionString, string databaseName)
        {
            EnsureSafeIdentifier(databaseName);

            var masterConnectionString = BuildMasterConnectionString(connectionString);
            var sql = $"CREATE DATABASE [{databaseName}];";

            await ExecuteNonQueryAsync(masterConnectionString, sql);
        }

        public async Task DeleteDatabaseAsync(string connectionString, string databaseName)
        {
            EnsureSafeIdentifier(databaseName);

            var masterConnectionString = BuildMasterConnectionString(connectionString);

            var sql = $@"
IF DB_ID(N'{databaseName}') IS NOT NULL
BEGIN
    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{databaseName}];
END";

            await ExecuteNonQueryAsync(masterConnectionString, sql);
        }

        public async Task<IReadOnlyList<string>> GetTablesAsync(string connectionString, string databaseName)
        {
            EnsureSafeIdentifier(databaseName);

            var databaseConnectionString = BuildDatabaseConnectionString(connectionString, databaseName);

            const string sql = @"
SELECT [name]
FROM sys.tables
ORDER BY [name];";

            var result = new List<string>();

            await using var connection = new SqlConnection(databaseConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }

            return result;
        }

        public async Task CreateTableAsync(string connectionString, string databaseName, string tableName)
        {
            EnsureSafeIdentifier(databaseName);
            EnsureSafeIdentifier(tableName);

            var databaseConnectionString = BuildDatabaseConnectionString(connectionString, databaseName);

            var sql = $@"
IF OBJECT_ID(N'dbo.[{tableName}]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[{tableName}]
    (
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_{tableName}] PRIMARY KEY
    );
END";

            await ExecuteNonQueryAsync(databaseConnectionString, sql);
        }

        public async Task DeleteTableAsync(string connectionString, string databaseName, string tableName)
        {
            EnsureSafeIdentifier(databaseName);
            EnsureSafeIdentifier(tableName);

            var databaseConnectionString = BuildDatabaseConnectionString(connectionString, databaseName);

            var sql = $@"
IF OBJECT_ID(N'dbo.[{tableName}]', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.[{tableName}];
END";

            await ExecuteNonQueryAsync(databaseConnectionString, sql);
        }

        private static string BuildDatabaseConnectionString(string connectionString, string databaseName)
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = databaseName,
                AttachDBFilename = string.Empty
            };

            return builder.ConnectionString;
        }

        private static async Task ExecuteNonQueryAsync(string connectionString, string sql)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private static string BuildMasterConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            builder.InitialCatalog = "master";
            builder.AttachDBFilename = string.Empty;

            return builder.ConnectionString;
        }

        private static void EnsureSafeIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be empty.", nameof(name));

            if (!Regex.IsMatch(name, "^[A-Za-z0-9_]+$"))
                throw new ArgumentException("Only letters, digits and underscore are allowed.", nameof(name));
        }

        #endregion
    }
}
