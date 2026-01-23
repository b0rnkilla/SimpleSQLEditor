using SimpleSQLEditor.Infrastructure;
using System.Data;

namespace SimpleSQLEditor.Services.DataAccess
{
    public interface IDataAccessService
    {
        string ProviderName { get; }

        Task<DataAccessResult<IReadOnlyList<string>>> GetDatabasesAsync(string connectionString);

        Task<DataAccessResult<IReadOnlyList<string>>> GetTablesAsync(string connectionString, string databaseName);

        Task<DataAccessResult<IReadOnlyDictionary<string, string>>> GetColumnDataTypesAsync(string connectionString, string databaseName, string tableName);

        Task<DataAccessResult<DataTable>> GetTableDataAsync(string connectionString, string databaseName, string tableName, int maxRows);
    }
}