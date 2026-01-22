namespace SimpleSQLEditor.Services
{
    public class SqlDatabaseCatalogService : IDatabaseCatalogService
    {
        #region Fields

        private readonly SqlServerAdminService _sqlServerAdminService;

        #endregion

        #region Properties
        
        public string ProviderName => "SQL";
        
        #endregion

        #region Constructor

        public SqlDatabaseCatalogService(SqlServerAdminService sqlServerAdminService)
        {
            _sqlServerAdminService = sqlServerAdminService;
        }

        #endregion

        #region Methods & Events

        public Task<IReadOnlyList<string>> GetDatabasesAsync(string connectionString)
        {
            return _sqlServerAdminService.GetDatabasesAsync(connectionString);
        }

        #endregion
    }
}