using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSQLEditor.Services;
using SimpleSQLEditor.Infrastructure;
using SimpleSQLEditor.Services.DataAccess;
using System.Data;

namespace SimpleSQLEditor.ViewModels
{
    public partial class TableDataViewModel : ObservableObject
    {
        #region Fields

        private readonly IDataAccessService _dataAccessService;

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

        public TableDataViewModel(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;

            _connectionString = string.Empty;
            _databaseName = string.Empty;
            _tableName = string.Empty;

            ReloadCommand = new AsyncRelayCommand(LoadAsync, CanReload);
        }

        #endregion

        #region Methods & Events

        public event EventHandler? LoadingStarted;

        public event EventHandler<int>? RowsLoaded;

        public event EventHandler<string>? LoadingFailed;

        public void Initialize(string connectionString, string databaseName, string tableName, int maxRows = 100)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            TableName = tableName;
            MaxRows = maxRows;
        }

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
                LoadingStarted?.Invoke(this, EventArgs.Empty);

                ErrorText = null;
                OnPropertyChanged(nameof(HasError));

                var result = await _dataAccessService.GetTableDataAsync(ConnectionString, DatabaseName, TableName, MaxRows);

                TableData = result.Data.DefaultView;
                RowsLoaded?.Invoke(this, TableData?.Count ?? 0);

            }
            catch (Exception ex)
            {
                TableData = null;
                ErrorText = ex.Message;
                LoadingFailed?.Invoke(this, ex.Message);
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
