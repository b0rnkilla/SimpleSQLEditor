using SimpleSQLEditor.Infrastructure;

namespace SimpleSQLEditor.Services
{
    public interface IDatabaseCatalogService
    {
        string ProviderName { get; }

        Task<DataAccessResult<IReadOnlyList<string>>> GetDatabasesAsync(string connectionString);
    }
}