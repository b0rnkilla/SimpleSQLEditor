using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        [ObservableProperty]
        private string? _errorText;

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorText);

        #endregion

        #region Commands

        public IAsyncRelayCommand ReloadCommand { get; }

        #endregion

        #region Constructor

        public TableDataViewModel(SqlServerAdminService sqlAdminService, string connectionString, string databaseName, string tableName, int maxRows = 100)
        {
            _sqlAdminService = sqlAdminService;

            _connectionString = connectionString;
            _databaseName = databaseName;
            _tableName = tableName;
            _maxRows = maxRows;

            ReloadCommand = new AsyncRelayCommand(LoadAsync, CanReload);
        }

        #endregion

        #region Methods & Events

        public async Task LoadAsync()
        {
            if (IsLoading)
                return;

            if (MaxRows <= 0)
            {
                TableData = null;
                ErrorText = "Max rows must be greater than 0.";
                OnPropertyChanged(nameof(HasError));
                return;
            }

            try
            {
                IsLoading = true;
                ErrorText = null;
                OnPropertyChanged(nameof(HasError));

                var dataTable = await _sqlAdminService.GetTableDataAsync(ConnectionString, DatabaseName, TableName, MaxRows);

                TableData = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                TableData = null;
                ErrorText = ex.Message;
                OnPropertyChanged(nameof(HasError));
            }
            finally
            {
                IsLoading = false;
                ReloadCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanReload()
        {
            return !IsLoading;
        }

        partial void OnIsLoadingChanged(bool value)
        {
            ReloadCommand.NotifyCanExecuteChanged();
        }

        #endregion
    }
}
