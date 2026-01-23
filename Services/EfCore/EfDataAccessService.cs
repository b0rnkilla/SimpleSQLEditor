using SimpleSQLEditor.Infrastructure;
using SimpleSQLEditor.Services.DataAccess;
using System.Data;

namespace SimpleSQLEditor.Services.EfCore
{
    public class EfDataAccessService : IDataAccessService
    {
        #region Fields

        private readonly EfDatabaseAdminService _efDatabaseAdminService;

        #endregion

        #region Properties

        public string ProviderName => "EF";

        #endregion

        #region Constructor

        public EfDataAccessService(EfDatabaseAdminService efDatabaseAdminService)
        {
            _efDatabaseAdminService = efDatabaseAdminService;
        }

        #endregion

        #region Methods & Events

        public async Task<DataAccessResult<IReadOnlyList<string>>> GetDatabasesAsync(string connectionString)
        {
            var data = await _efDatabaseAdminService.GetDatabasesAsync(connectionString);

            return new DataAccessResult<IReadOnlyList<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<IReadOnlyList<string>>> GetTablesAsync(string connectionString, string databaseName)
        {
            var data = await _efDatabaseAdminService.GetTablesAsync(connectionString, databaseName);

            return new DataAccessResult<IReadOnlyList<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<IReadOnlyDictionary<string, string>>> GetColumnDataTypesAsync(string connectionString, string databaseName, string tableName)
        {
            var data = await _efDatabaseAdminService.GetColumnDataTypesAsync(connectionString, databaseName, tableName);

            return new DataAccessResult<IReadOnlyDictionary<string, string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<IReadOnlyCollection<string>>> GetPrimaryKeyColumnsAsync(string connectionString, string databaseName, string tableName)
        {
            var data = await _efDatabaseAdminService.GetPrimaryKeyColumnsAsync(connectionString, databaseName, tableName);

            return new DataAccessResult<IReadOnlyCollection<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<IReadOnlyCollection<string>>> GetForeignKeyColumnsAsync(string connectionString, string databaseName, string tableName)
        {
            var data = await _efDatabaseAdminService.GetForeignKeyColumnsAsync(connectionString, databaseName, tableName);

            return new DataAccessResult<IReadOnlyCollection<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        public async Task<DataAccessResult<DataTable>> GetTableDataAsync(string connectionString, string databaseName, string tableName, int maxRows)
        {
            var data = await _efDatabaseAdminService.GetTableDataAsync(connectionString, databaseName, tableName, maxRows);

            return new DataAccessResult<DataTable>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        #endregion
    }
}