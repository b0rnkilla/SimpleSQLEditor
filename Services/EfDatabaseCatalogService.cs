using SimpleSQLEditor.Infrastructure;
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

        public async Task<DataAccessResult<IReadOnlyList<string>>> GetDatabasesAsync(string connectionString)
        {
            var data = await _efDatabaseQueryService.GetUserDatabasesAsync(connectionString);

            return new DataAccessResult<IReadOnlyList<string>>
            {
                Provider = ProviderName,
                Data = data
            };
        }

        #endregion
    }
}