using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using SimpleSQLEditor.Infrastructure;
using System.Data;

namespace SimpleSQLEditor.Services
{
    public class SqlServerAdminService
    {
        #region Fields

        private const int DefaultCommandTimeoutSeconds = 30;

        #endregion

        #region Methods

        public async Task TestConnectionAsync(string connectionString)
        {
            var masterConnectionString = BuildMasterConnectionString(connectionString);

            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();
        }

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

            await using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = DefaultCommandTimeoutSeconds
            };
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }

            return result;
        }

        public async Task<bool> CreateDatabaseAsync(string connectionString, string databaseName)
        {
            EnsureSafeIdentifier(databaseName);

            var masterConnectionString = BuildMasterConnectionString(connectionString);

            var sql = $@"
IF DB_ID(N'{databaseName}') IS NULL
BEGIN
    EXEC('CREATE DATABASE [{databaseName}]');
    SELECT CAST(1 AS bit);
END
ELSE
BEGIN
    SELECT CAST(0 AS bit);
END";

            return await ExecuteScalarBoolAsync(masterConnectionString, sql);
        }

        public async Task<bool> DeleteDatabaseAsync(string connectionString, string databaseName)
        {
            EnsureSafeIdentifier(databaseName);

            var masterConnectionString = BuildMasterConnectionString(connectionString);

            var sql = $@"
IF DB_ID(N'{databaseName}') IS NOT NULL
BEGIN
    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{databaseName}];
    SELECT CAST(1 AS bit);
END
ELSE
BEGIN
    SELECT CAST(0 AS bit);
END";

            return await ExecuteScalarBoolAsync(masterConnectionString, sql);
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

            await using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = DefaultCommandTimeoutSeconds
            };
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }

            return result;
        }

        public async Task<bool> CreateTableAsync(string connectionString, string databaseName, string tableName)
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
    SELECT CAST(1 AS bit);
END
ELSE
BEGIN
    SELECT CAST(0 AS bit);
END";

            return await ExecuteScalarBoolAsync(databaseConnectionString, sql);
        }

        public async Task<bool> DeleteTableAsync(string connectionString, string databaseName, string tableName)
        {
            EnsureSafeIdentifier(databaseName);
            EnsureSafeIdentifier(tableName);

            var databaseConnectionString = BuildDatabaseConnectionString(connectionString, databaseName);

            var sql = $@"
IF OBJECT_ID(N'dbo.[{tableName}]', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.[{tableName}];
    SELECT CAST(1 AS bit);
END
ELSE
BEGIN
    SELECT CAST(0 AS bit);
END";

            return await ExecuteScalarBoolAsync(databaseConnectionString, sql);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetColumnDataTypesAsync(string connectionString, string databaseName, string tableName)
        {
            EnsureSafeIdentifier(databaseName);
            EnsureSafeIdentifier(tableName);

            var databaseConnectionString = BuildDatabaseConnectionString(connectionString, databaseName);

            const string sql = @"
SELECT 
    c.[name] AS ColumnName,
    t.[name] AS TypeName,
    c.max_length,
    c.[precision],
    c.[scale]
FROM sys.columns c
INNER JOIN sys.tables tb ON tb.[object_id] = c.[object_id]
INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE tb.[name] = @TableName
ORDER BY c.column_id;";

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            await using var connection = new SqlConnection(databaseConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = DefaultCommandTimeoutSeconds
            };
            command.Parameters.AddWithValue("@TableName", tableName);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var columnName = reader.GetString(0);
                var typeName = reader.GetString(1);
                var maxLength = reader.GetInt16(2);
                var precision = reader.GetByte(3);
                var scale = reader.GetByte(4);

                var formattedType = FormatSqlType(typeName, maxLength, precision, scale);
                result[columnName] = formattedType;
            }

            return result;
        }

        public async Task<bool> CreateColumnAsync(string connectionString, string databaseName, string tableName, string columnName, string dataType)
        {
            EnsureSafeIdentifier(databaseName);
            EnsureSafeIdentifier(tableName);
            EnsureSafeIdentifier(columnName);
            EnsureSafeDataType(dataType);

            var databaseConnectionString = BuildDatabaseConnectionString(connectionString, databaseName);

            var sql = $@"
IF COL_LENGTH(N'dbo.[{tableName}]', N'{columnName}') IS NULL
BEGIN
    ALTER TABLE dbo.[{tableName}] ADD [{columnName}] {dataType} NULL;
    SELECT CAST(1 AS bit);
END
ELSE
BEGIN
    SELECT CAST(0 AS bit);
END";

            return await ExecuteScalarBoolAsync(databaseConnectionString, sql);
        }

        public async Task<bool> DeleteColumnAsync(string connectionString, string databaseName, string tableName, string columnName)
        {
            EnsureSafeIdentifier(databaseName);
            EnsureSafeIdentifier(tableName);
            EnsureSafeIdentifier(columnName);

            var databaseConnectionString = BuildDatabaseConnectionString(connectionString, databaseName);

            var sql = $@"
IF COL_LENGTH(N'dbo.[{tableName}]', N'{columnName}') IS NOT NULL
BEGIN
    ALTER TABLE dbo.[{tableName}] DROP COLUMN [{columnName}];
    SELECT CAST(1 AS bit);
END
ELSE
BEGIN
    SELECT CAST(0 AS bit);
END";

            return await ExecuteScalarBoolAsync(databaseConnectionString, sql);
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

        private static async Task<bool> ExecuteScalarBoolAsync(string connectionString, string sql)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = DefaultCommandTimeoutSeconds
            };

            var result = await command.ExecuteScalarAsync();

            return result is bool b && b;
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

        private static void EnsureSafeDataType(string dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType))
                throw new ArgumentException("Data type must not be empty.", nameof(dataType));

            if (!SqlDataTypes.Allowed.Contains(dataType.Trim(), StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Data type is not allowed.", nameof(dataType));
        }

        private static string FormatSqlType(string typeName, short maxLength, byte precision, byte scale)
        {
            if (typeName.Equals("nvarchar", StringComparison.OrdinalIgnoreCase) ||
                typeName.Equals("nchar", StringComparison.OrdinalIgnoreCase))
            {
                if (maxLength == -1)
                    return $"{typeName}(max)";

                var chars = maxLength / 2;
                return $"{typeName}({chars})";
            }

            if (typeName.Equals("varchar", StringComparison.OrdinalIgnoreCase) ||
                typeName.Equals("char", StringComparison.OrdinalIgnoreCase))
            {
                if (maxLength == -1)
                    return $"{typeName}(max)";

                return $"{typeName}({maxLength})";
            }

            if (typeName.Equals("decimal", StringComparison.OrdinalIgnoreCase) ||
                typeName.Equals("numeric", StringComparison.OrdinalIgnoreCase))
            {
                return $"{typeName}({precision},{scale})";
            }

            return typeName;
        }

        public async Task<DataTable> GetTableDataAsync(string connectionString, string databaseName, string tableName, int maxRows)
        {
            EnsureSafeIdentifier(databaseName);
            EnsureSafeIdentifier(tableName);

            if (maxRows <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxRows), "Max rows must be greater than 0.");

            var databaseConnectionString = BuildDatabaseConnectionString(connectionString, databaseName);

            var sql = $@"
SELECT TOP (@MaxRows) *
FROM dbo.[{tableName}];";

            await using var connection = new SqlConnection(databaseConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = DefaultCommandTimeoutSeconds
            };

            command.Parameters.AddWithValue("@MaxRows", maxRows);

            await using var reader = await command.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);

            return table;
        }

        #endregion
    }
}
