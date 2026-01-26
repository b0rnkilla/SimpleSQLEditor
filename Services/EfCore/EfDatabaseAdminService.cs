using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace SimpleSQLEditor.Services.EfCore
{
    public class EfDatabaseAdminService
    {
        #region Fields

        private const int DEFAULT_COMMAND_TIMEOUT_SECONDS = 30;

        #endregion

        #region Methods & Events

        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            var masterConnectionString = BuildConnectionString(connectionString);
            await using var context = CreateContext(masterConnectionString);

            await context.Database.OpenConnectionAsync();
            await context.Database.CloseConnectionAsync();

            return true;
        }

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

        public async Task<EfRowTrackingSession> StartRowTrackingAsync(
            string connectionString, string databaseName, string tableName,
            string primaryKeyColumn, object primaryKeyValue, DataTable rowDataTable)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name must not be empty.", nameof(databaseName));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be empty.", nameof(tableName));

            if (string.IsNullOrWhiteSpace(primaryKeyColumn))
                throw new ArgumentException("Primary key column must not be empty.", nameof(primaryKeyColumn));

            var databaseConnectionString = BuildConnectionString(connectionString, databaseName);

            var columnTypes = rowDataTable.Columns
                .Cast<DataColumn>()
                .ToDictionary(c => c.ColumnName, c => c.DataType, StringComparer.OrdinalIgnoreCase);

            var descriptor = new TrackingModelDescriptor
            {
                EntityName = BuildSafeEntityName(databaseName, tableName),
                TableName = tableName,
                PrimaryKeyColumn = primaryKeyColumn,
                ColumnTypes = columnTypes
            };

            var options = new DbContextOptionsBuilder<EfDbContext>()
                .UseSqlServer(databaseConnectionString)
                .ReplaceService<IModelCacheKeyFactory, TrackingModelCacheKeyFactory>()
                .Options;

            var context = new EfDbContext(options, descriptor);

            var set = context.Set<Dictionary<string, object>>(descriptor.EntityName);

            if (descriptor.ColumnTypes.TryGetValue(primaryKeyColumn, out var pkType) && primaryKeyValue is not null)
                primaryKeyValue = Convert.ChangeType(primaryKeyValue, pkType);

            var entity = await set.FindAsync(primaryKeyValue);

            if (entity is null)
            {
                context.Dispose();
                throw new InvalidOperationException("Selected row could not be loaded as tracked entity.");
            }

            return new EfRowTrackingSession(context, entity);
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

        private static string BuildSafeEntityName(string databaseName, string tableName)
        {
            static string Sanitize(string value)
            {
                return Regex.Replace(value, @"[^A-Za-z0-9_]", "_");
            }

            return $"TrackedRow_{Sanitize(databaseName)}_{Sanitize(tableName)}";
        }

        #endregion

        #region Nested Classes

        public sealed class EfTrackingSnapshot
        {
            public required string State { get; init; }

            public required IReadOnlyList<string> ModifiedColumns { get; init; }
        }

        public sealed class EfRowTrackingSession : IDisposable
        {
            #region Fields

            private readonly EfDbContext _context;

            private readonly Dictionary<string, object> _entity;

            #endregion

            #region Constructor

            internal EfRowTrackingSession(EfDbContext context, Dictionary<string, object> entity)
            {
                _context = context;
                _entity = entity;
            }

            #endregion

            #region Methods & Events

            public EfTrackingSnapshot GetSnapshot()
            {
                EntityEntry entry = _context.Entry(_entity);

                var modified = entry.Properties
                    .Where(p => p.IsModified)
                    .Select(p => p.Metadata.Name)
                    .ToList();

                return new EfTrackingSnapshot
                {
                    State = entry.State.ToString(),
                    ModifiedColumns = modified
                };
            }

            public void SetValue(string columnName, object? value)
            {
                if (string.IsNullOrWhiteSpace(columnName))
                    throw new ArgumentException("Column name must not be empty.", nameof(columnName));

                if (!_entity.ContainsKey(columnName))
                    throw new ArgumentException("Column does not exist in tracked entity.", nameof(columnName));

                _entity[columnName] = value ?? DBNull.Value;

                var entry = _context.Entry(_entity);
                entry.Property(columnName).IsModified = true;
            }

            public void RevertChanges()
            {
                var entry = _context.Entry(_entity);

                entry.CurrentValues.SetValues(entry.OriginalValues);

                foreach (var property in entry.Properties)
                {
                    property.IsModified = false;
                }

                entry.State = EntityState.Unchanged;
            }

            public void Dispose()
            {
                _context.Dispose();
            }

            #endregion
        }

        private sealed class TrackingModelCacheKeyFactory : IModelCacheKeyFactory
        {
            public object Create(DbContext context, bool designTime)
            {
                if (context is EfDbContext efContext && efContext.TrackingCacheKey is not null)
                    return (context.GetType(), efContext.TrackingCacheKey, designTime);

                return (context.GetType(), designTime);
            }
        }

        #endregion
    }
}