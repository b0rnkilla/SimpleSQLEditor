using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace SimpleSQLEditor.Services.EfCore
{
    public class EfDatabaseAdminService
    {
        #region Fields

        private readonly IEfRuntimeContextFactory _contextFactory;

        private const int DEFAULT_COMMAND_TIMEOUT_SECONDS = 30;

        #endregion

        #region Constructor

        public EfDatabaseAdminService(IEfRuntimeContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #endregion

        #region Methods & Events

        public async Task<IReadOnlyList<string>> GetDatabasesAsync(string connectionString)
        {
            var masterConnectionString = BuildConnectionString(connectionString);

            await using var context = _contextFactory.Create(masterConnectionString);

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

        public async Task<DataTable> GetTableDataAsync(string connectionString, string databaseName, string tableName, int maxRows)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name must not be empty.", nameof(databaseName));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must not be empty.", nameof(tableName));

            if (maxRows <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxRows), "Max rows must be greater than 0.");

            var databaseConnectionString = BuildConnectionString(connectionString, databaseName);

            await using var context = _contextFactory.Create(databaseConnectionString);

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

        #endregion
    }
}