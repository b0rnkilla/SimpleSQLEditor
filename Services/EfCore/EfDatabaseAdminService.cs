using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace SimpleSQLEditor.Services.EfCore
{
    public class EfDatabaseAdminService
    {
        #region Fields

        private const int DEFAULT_COMMAND_TIMEOUT_SECONDS = 30;

        #endregion

        #region Methods & Events

        public async Task<IReadOnlyList<string>> GetDatabasesAsync(string connectionString)
        {
            var masterConnectionString = BuildConnectionString(connectionString);
            await using var context = CreateContext(masterConnectionString);

            const string sql = @"
SELECT [name]
FROM sys.databases
WHERE [name] NOT IN ('master', 'model', 'msdb', 'tempdb')";

            var result = await context.DatabaseNames
                .FromSqlRaw(sql)
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .ToListAsync();

            return result;
        }

        public async Task<IReadOnlyList<string>> GetTablesAsync(string connectionString, string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name must not be empty.", nameof(databaseName));

            var databaseConnectionString = BuildConnectionString(connectionString, databaseName);
            await using var context = CreateContext(databaseConnectionString);

            const string sql = @"
SELECT [name]
FROM sys.tables
ORDER BY [name];";

            var result = await context.Database
                .SqlQueryRaw<string>(sql)
                .ToListAsync();

            return result;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetColumnDataTypesAsync(string connectionString, string databaseName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name must not be empty.", nameof(databaseName));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be empty.", nameof(tableName));

            var databaseConnectionString = BuildConnectionString(connectionString, databaseName);
            await using var context = CreateContext(databaseConnectionString);

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

            var connection = context.Database.GetDbConnection();
            await EnsureOpenAsync(connection);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = DEFAULT_COMMAND_TIMEOUT_SECONDS;

            var tableNameParameter = command.CreateParameter();
            tableNameParameter.ParameterName = "@TableName";
            tableNameParameter.Value = tableName;
            command.Parameters.Add(tableNameParameter);

            await using var reader = await command.ExecuteReaderAsync();

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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

        public async Task<IReadOnlyCollection<string>> GetPrimaryKeyColumnsAsync(string connectionString, string databaseName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name must not be empty.", nameof(databaseName));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be empty.", nameof(tableName));

            var databaseConnectionString = BuildConnectionString(connectionString, databaseName);
            await using var context = CreateContext(databaseConnectionString);

            const string sql = @"
SELECT c.[name]
FROM sys.tables t
INNER JOIN sys.schemas s ON s.[schema_id] = t.[schema_id]
INNER JOIN sys.indexes i ON i.[object_id] = t.[object_id] AND i.[is_primary_key] = 1
INNER JOIN sys.index_columns ic ON ic.[object_id] = i.[object_id] AND ic.[index_id] = i.[index_id]
INNER JOIN sys.columns c ON c.[object_id] = t.[object_id] AND c.[column_id] = ic.[column_id]
WHERE s.[name] = N'dbo'
  AND t.[name] = @TableName
ORDER BY ic.[key_ordinal];";

            var connection = context.Database.GetDbConnection();
            await EnsureOpenAsync(connection);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = DEFAULT_COMMAND_TIMEOUT_SECONDS;

            var tableNameParameter = command.CreateParameter();
            tableNameParameter.ParameterName = "@TableName";
            tableNameParameter.Value = tableName;
            command.Parameters.Add(tableNameParameter);

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }

            return result;
        }

        public async Task<IReadOnlyCollection<string>> GetForeignKeyColumnsAsync(string connectionString, string databaseName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name must not be empty.", nameof(databaseName));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be empty.", nameof(tableName));

            var databaseConnectionString = BuildConnectionString(connectionString, databaseName);
            await using var context = CreateContext(databaseConnectionString);

            const string sql = @"
SELECT DISTINCT pc.[name]
FROM sys.tables t
INNER JOIN sys.schemas s ON s.[schema_id] = t.[schema_id]
INNER JOIN sys.foreign_key_columns fkc ON fkc.[parent_object_id] = t.[object_id]
INNER JOIN sys.columns pc ON pc.[object_id] = fkc.[parent_object_id] AND pc.[column_id] = fkc.[parent_column_id]
WHERE s.[name] = N'dbo'
  AND t.[name] = @TableName
ORDER BY pc.[name];";

            var connection = context.Database.GetDbConnection();
            await EnsureOpenAsync(connection);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = DEFAULT_COMMAND_TIMEOUT_SECONDS;

            var tableNameParameter = command.CreateParameter();
            tableNameParameter.ParameterName = "@TableName";
            tableNameParameter.Value = tableName;
            command.Parameters.Add(tableNameParameter);

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }

            return result;
        }

        public async Task<DataTable> GetTableDataAsync(string connectionString, string databaseName, string tableName, int maxRows)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name must not be empty.", nameof(databaseName));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be empty.", nameof(tableName));

            if (maxRows <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxRows), "Max rows must be greater than 0.");

            var databaseConnectionString = BuildConnectionString(connectionString, databaseName);
            await using var context = CreateContext(databaseConnectionString);

            var sql = $@"
SELECT TOP (@MaxRows) *
FROM dbo.[{tableName}];";

            var connection = context.Database.GetDbConnection();

            await EnsureOpenAsync(connection);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = DEFAULT_COMMAND_TIMEOUT_SECONDS;

            var maxRowsParameter = command.CreateParameter();
            maxRowsParameter.ParameterName = "@MaxRows";
            maxRowsParameter.Value = maxRows;
            command.Parameters.Add(maxRowsParameter);

            await using var reader = await command.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);

            return table;
        }

        private static async Task EnsureOpenAsync(DbConnection connection)
        {
            if (connection.State == ConnectionState.Open)
                return;

            await connection.OpenAsync();
        }

        private static string BuildConnectionString(string connectionString, string? databaseName = null)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            builder.InitialCatalog = string.IsNullOrWhiteSpace(databaseName)
                ? "master"
                : databaseName;

            builder.AttachDBFilename = string.Empty;

            return builder.ConnectionString;
        }

        private static EfDbContext CreateContext(string connectionString)
        {
            var options = new DbContextOptionsBuilder<EfDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new EfDbContext(options);
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

        #endregion
    }
}