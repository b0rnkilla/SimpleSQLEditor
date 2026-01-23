using SimpleSQLEditor.Infrastructure;
using SimpleSQLEditor.Services.DataAccess;
using System.Data;

namespace SimpleSQLEditor.Services.Sql
{
    public class SqlDataAccessService : IDataAccessService
    {
        #region Fields

        private readonly SqlServerAdminService _sqlServerAdminService;

        #endregion

        #region Properties

        public string ProviderName => "SQL";

        #endregion

        #region Constructor

        public SqlDataAccessService(SqlServerAdminService sqlServerAdminService)
        {
            _sqlServerAdminService = sqlServerAdminService;
        }

        #endregion

        #region Methods & Events

        public async Task<DataAccessResult<bool>> TestConnectionAsync(string connectionString)
        {
            await _sqlServerAdminService.TestConnectionAsync(connectionString);

            return new DataAccessResult<bool>
            {
                Provider = ProviderName,
                Data = true
            };
        }

        public async Task<DataAccessResult<IReadOnlyList<string>>> GetDatabasesAsync(string connectionString)
        {
            var data = await _sqlServerAdminService.GetDatabasesAsync(connectionString);

            return new DataAccessResult<IReadOnlyList<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<IReadOnlyList<string>>> GetTablesAsync(string connectionString, string databaseName)
        {
            var data = await _sqlServerAdminService.GetTablesAsync(connectionString, databaseName);

            return new DataAccessResult<IReadOnlyList<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<IReadOnlyDictionary<string, string>>> GetColumnDataTypesAsync(string connectionString, string databaseName, string tableName)
        {
            var data = await _sqlServerAdminService.GetColumnDataTypesAsync(connectionString, databaseName, tableName);

            return new DataAccessResult<IReadOnlyDictionary<string, string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<IReadOnlyCollection<string>>> GetPrimaryKeyColumnsAsync(string connectionString, string databaseName, string tableName)
        {
            var data = await _sqlServerAdminService.GetPrimaryKeyColumnsAsync(connectionString, databaseName, tableName);

            return new DataAccessResult<IReadOnlyCollection<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<IReadOnlyCollection<string>>> GetForeignKeyColumnsAsync(string connectionString, string databaseName, string tableName)
        {
            var data = await _sqlServerAdminService.GetForeignKeyColumnsAsync(connectionString, databaseName, tableName);

            return new DataAccessResult<IReadOnlyCollection<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<DataTable>> GetTableDataAsync(string connectionString, string databaseName, string tableName, int maxRows)
        {
            var data = await _sqlServerAdminService.GetTableDataAsync(connectionString, databaseName, tableName, maxRows);

            return new DataAccessResult<DataTable>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        #endregion
    }
}