using CommunityToolkit.Mvvm.ComponentModel;
using SimpleSQLEditor.Services;
using System.Data;

namespace SimpleSQLEditor.ViewModels
{
    public partial class TableDataViewModel : ObservableObject
    {
        #region Fields

        private readonly SqlServerAdminService _sqlAdminService;

        #endregion

        #region Properties

        [ObservableProperty]
        private string _connectionString;

        [ObservableProperty]
        private string _databaseName;

        [ObservableProperty]
        private string _tableName;

        [ObservableProperty]
        private int _maxRows = 100;

        [ObservableProperty]
        private DataView? _tableData;

        [ObservableProperty]
        private bool _isLoading;

        #endregion

        #region Constructor

        public TableDataViewModel(SqlServerAdminService sqlAdminService, string connectionString, string databaseName, string tableName, int maxRows = 100)
        {
            _sqlAdminService = sqlAdminService;

            _connectionString = connectionString;
            _databaseName = databaseName;
            _tableName = tableName;
            _maxRows = maxRows;
        }

        #endregion

        #region Methods & Events

        public async Task LoadAsync()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;

                var dataTable = await _sqlAdminService.GetTableDataAsync(
                    ConnectionString,
                    DatabaseName,
                    TableName,
                    MaxRows);

                TableData = dataTable.DefaultView;
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}
