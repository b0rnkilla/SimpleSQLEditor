namespace SimpleSQLEditor.Services.Ui
{
    public interface IColumnDefinitionService
    {
        event EventHandler<string> DataTypeInsertRequested;

        void RequestInsertDataType(string dataType);
    }
}