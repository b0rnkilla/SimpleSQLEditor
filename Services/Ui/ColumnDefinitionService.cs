namespace SimpleSQLEditor.Services.Ui
{
    public class ColumnDefinitionService : IColumnDefinitionService
    {
        #region Methods & Events

        public event EventHandler<string>? DataTypeInsertRequested;

        public void RequestInsertDataType(string dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType))
                return;

            DataTypeInsertRequested?.Invoke(this, dataType.Trim());
        }

        #endregion
    }
}