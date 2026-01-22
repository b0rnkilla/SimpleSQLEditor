using SimpleSQLEditor.Infrastructure;

namespace SimpleSQLEditor.Services
{
    public interface IDataAccessModeService
    {
        DataAccessMode CurrentMode { get; set; }
    }
}