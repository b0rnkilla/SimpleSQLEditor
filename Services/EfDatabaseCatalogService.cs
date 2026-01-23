using SimpleSQLEditor.Infrastructure;
using SimpleSQLEditor.Services.EfCore;

namespace SimpleSQLEditor.Services
{
    public class EfDatabaseCatalogService : IDatabaseCatalogService
    {
        #region Fields

        private readonly EfDatabaseAdminService _efDatabaseAdminService;

        #endregion

        #region Properties

        public string ProviderName => "EF";

        #endregion

        #region Constructor

        public EfDatabaseCatalogService(EfDatabaseAdminService efDatabaseAdminService)
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

        #endregion
    }
}