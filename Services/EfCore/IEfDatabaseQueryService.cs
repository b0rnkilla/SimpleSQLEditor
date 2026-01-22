namespace SimpleSQLEditor.Services.EfCore
{
    public interface IEfDatabaseQueryService
    {
        Task<IReadOnlyList<string>> GetUserDatabasesAsync(string connectionString);
    }
}