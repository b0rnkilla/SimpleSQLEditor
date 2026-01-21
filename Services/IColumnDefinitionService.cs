namespace SimpleSQLEditor.Services
{
    public interface IColumnDefinitionService
    {
        event EventHandler<string> DataTypeInsertRequested;

        void RequestInsertDataType(string dataType);
    }
}