using SimpleSQLEditor.Infrastructure;

namespace SimpleSQLEditor.Services.State
{
    public interface IDataAccessModeService
    {
        DataAccessMode CurrentMode { get; set; }
    }
}