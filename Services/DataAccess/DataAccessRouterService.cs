using SimpleSQLEditor.Infrastructure;
using SimpleSQLEditor.Services.EfCore;
using SimpleSQLEditor.Services.Sql;
using SimpleSQLEditor.Services.State;
using System.Data;

namespace SimpleSQLEditor.Services.DataAccess
{
    public class DataAccessRouterService : IDataAccessService
    {
        #region Fields

        private readonly IDataAccessModeService _dataAccessModeService;

        private readonly SqlDataAccessService _sqlDataAccessService;

        private readonly EfDataAccessService _efDataAccessService;

        #endregion

        #region Properties

        public string ProviderName => GetActiveService().ProviderName;

        #endregion

        #region Constructor

        public DataAccessRouterService(
            IDataAccessModeService dataAccessModeService,
            SqlDataAccessService sqlDataAccessService,
            EfDataAccessService efDataAccessService)
        {
            _dataAccessModeService = dataAccessModeService;
            _sqlDataAccessService = sqlDataAccessService;
            _efDataAccessService = efDataAccessService;
        }

        #endregion

        #region Methods & Events

        public Task<DataAccessResult<IReadOnlyList<string>>> GetDatabasesAsync(string connectionString)
        {
            return GetActiveService().GetDatabasesAsync(connectionString);
        }

        public Task<DataAccessResult<IReadOnlyList<string>>> GetTablesAsync(string connectionString, string databaseName)
        {
            return GetActiveService().GetTablesAsync(connectionString, databaseName);
        }

        public Task<DataAccessResult<IReadOnlyDictionary<string, string>>> GetColumnDataTypesAsync(string connectionString, string databaseName, string tableName)
        {
            return GetActiveService().GetColumnDataTypesAsync(connectionString, databaseName, tableName);
        }

        public Task<DataAccessResult<IReadOnlyCollection<string>>> GetPrimaryKeyColumnsAsync(string connectionString, string databaseName, string tableName)
        {
            return GetActiveService().GetPrimaryKeyColumnsAsync(connectionString, databaseName, tableName);
        }

        public Task<DataAccessResult<IReadOnlyCollection<string>>> GetForeignKeyColumnsAsync(string connectionString, string databaseName, string tableName)
        {
            return GetActiveService().GetForeignKeyColumnsAsync(connectionString, databaseName, tableName);
        }

        public Task<DataAccessResult<DataTable>> GetTableDataAsync(string connectionString, string databaseName, string tableName, int maxRows)
        {
            return GetActiveService().GetTableDataAsync(connectionString, databaseName, tableName, maxRows);
        }

        private IDataAccessService GetActiveService()
        {
            return _dataAccessModeService.CurrentMode == DataAccessMode.Ef
                ? _efDataAccessService
                : _sqlDataAccessService;
        }

        #endregion
    }
}