using SimpleSQLEditor.Infrastructure;

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

        public async Task<DataAccessResult<IReadOnlyList<string>>> GetDatabasesAsync(string connectionString)
        {
            var data = await _sqlServerAdminService.GetDatabasesAsync(connectionString);

            return new DataAccessResult<IReadOnlyList<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        #endregion
    }
}