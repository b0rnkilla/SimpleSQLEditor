namespace SimpleSQLEditor.Services
{
    public interface IOperationSourceService
    {
        string CurrentSource { get; }

        IDisposable Begin(string source);
    }
}