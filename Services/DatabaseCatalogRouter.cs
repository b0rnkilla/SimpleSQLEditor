using SimpleSQLEditor.Infrastructure;

namespace SimpleSQLEditor.Services
{
    public class DatabaseCatalogRouter : IDatabaseCatalogService
    {
        #region Fields

        private readonly IDataAccessModeService _dataAccessModeService;

        private readonly SqlDatabaseCatalogService _sqlCatalogService;

        private readonly EfDatabaseCatalogService _efCatalogService;

        #endregion

        #region Properties

        public string ProviderName => GetActiveService().ProviderName;

        #endregion

        #region Constructor

        public DatabaseCatalogRouter(
            IDataAccessModeService dataAccessModeService,
            SqlDatabaseCatalogService sqlCatalogService,
            EfDatabaseCatalogService efCatalogService)
        {
            _dataAccessModeService = dataAccessModeService;
            _sqlCatalogService = sqlCatalogService;
            _efCatalogService = efCatalogService;
        }

        #endregion

        #region Methods & Events

        public Task<DataAccessResult<IReadOnlyList<string>>> GetDatabasesAsync(string connectionString)
        {
            return GetActiveService().GetDatabasesAsync(connectionString);
        }

        private IDatabaseCatalogService GetActiveService()
        {
            return _dataAccessModeService.CurrentMode == DataAccessMode.Ef
                ? _efCatalogService
                : _sqlCatalogService;
        }

        #endregion
    }
}