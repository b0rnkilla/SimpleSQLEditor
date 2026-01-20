namespace SimpleSQLEditor.Services
{
    public interface IWindowService
    {
        event EventHandler<bool> IsStatusLogOpenChanged;
        void ShowStatusLog();
    }
}
