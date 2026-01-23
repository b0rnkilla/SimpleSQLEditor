namespace SimpleSQLEditor.Services.State
{
    public interface IOperationSourceService
    {
        string CurrentSource { get; }

        IDisposable Begin(string source);
    }
}