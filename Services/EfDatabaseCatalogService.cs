using SimpleSQLEditor.Services.EfCore;

namespace SimpleSQLEditor.Services
{
    public class EfDatabaseCatalogService : IDatabaseCatalogService
    {
        #region Fields

        private readonly IEfDatabaseQueryService _efDatabaseQueryService;

        #endregion

        #region Properties

        public string ProviderName => "EF";

        #endregion

        #region Constructor

        public EfDatabaseCatalogService(IEfDatabaseQueryService efDatabaseQueryService)
        {
            _efDatabaseQueryService = efDatabaseQueryService;
        }

        #endregion

        #region Methods & Events

        public Task<IReadOnlyList<string>> GetDatabasesAsync(string connectionString)
        {
            return _efDatabaseQueryService.GetUserDatabasesAsync(connectionString);
        }

        #endregion
    }
}