using SimpleSQLEditor.Infrastructure;

namespace SimpleSQLEditor.Services
{
    public class DataAccessModeService : IDataAccessModeService
    {
        public DataAccessMode CurrentMode { get; set; } = DataAccessMode.Sql;
    }
}