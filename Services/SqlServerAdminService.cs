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

            // NEU: Force single user, rollback open transactions, then drop
            var sql = $@"
IF DB_ID(N'{databaseName}') IS NOT NULL
BEGIN
    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{databaseName}];
END";

            await ExecuteNonQueryAsync(masterConnectionString, sql);
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

            // NEU: Für Admin-Operationen immer auf master gehen
            builder.InitialCatalog = "master";
            builder.AttachDBFilename = string.Empty;

            return builder.ConnectionString;
        }

        private static void EnsureSafeIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be empty.", nameof(name));

            // NEU: Nur Buchstaben, Zahlen, Unterstrich. Kein Leerzeichen, kein Minus etc.
            // Für den Anfang bewusst restriktiv, um DDL-Injection zu vermeiden.
            if (!Regex.IsMatch(name, "^[A-Za-z0-9_]+$"))
                throw new ArgumentException("Only letters, digits and underscore are allowed.", nameof(name));
        }

        #endregion
    }
}
