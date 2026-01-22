using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace SimpleSQLEditor.Services.EfCore
{
    public class EfDatabaseQueryService : IEfDatabaseQueryService
    {
        #region Fields

        private readonly IEfRuntimeContextFactory _contextFactory;

        #endregion

        #region Constructor

        public EfDatabaseQueryService(IEfRuntimeContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #endregion

        #region Methods & Events

        public async Task<IReadOnlyList<string>> GetUserDatabasesAsync(string connectionString)
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