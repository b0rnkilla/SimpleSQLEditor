using SimpleSQLEditor.Infrastructure;

namespace SimpleSQLEditor.Services.State
{
    public class DataAccessModeService : IDataAccessModeService
    {
        public DataAccessMode CurrentMode { get; set; } = DataAccessMode.Sql;
    }
}