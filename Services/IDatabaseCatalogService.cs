namespace SimpleSQLEditor.Services
{
    public interface IDatabaseCatalogService
    {
        string ProviderName { get; }

        Task<IReadOnlyList<string>> GetDatabasesAsync(string connectionString);
    }
}