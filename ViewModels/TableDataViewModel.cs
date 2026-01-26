using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSQLEditor.Services.DataAccess;
using System.Collections.ObjectModel;
using System.Data;

namespace SimpleSQLEditor.ViewModels
{
    public partial class TableDataViewModel : ObservableObject
    {
        #region Fields

        private readonly IDataAccessService _dataAccessService;

        private readonly HashSet<string> _primaryKeyColumns = new(StringComparer.OrdinalIgnoreCase);

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

        [ObservableProperty]
        private DataRowView? _selectedRow;

        public ObservableCollection<RowDetailItem> RowDetails { get; } = new();

        public bool HasSelectedRow => SelectedRow is not null;

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
                SelectedRow = null;
                UpdateRowDetails(null);
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

                SelectedRow = null;
                UpdateRowDetails(null);
                OnPropertyChanged(nameof(HasSelectedRow));

                var result = await _dataAccessService.GetTableDataAsync(ConnectionString, DatabaseName, TableName, MaxRows);

                TableData = result.Data.DefaultView;

                SelectedRow = null;
                UpdateRowDetails(null);

                await LoadPrimaryKeyColumnsAsync();
                UpdateRowDetails(SelectedRow);

                RowsLoaded?.Invoke(this, TableData?.Count ?? 0);
            }
            catch (Exception ex)
            {
                TableData = null;
                SelectedRow = null;
                UpdateRowDetails(null);
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

        partial void OnSelectedRowChanged(DataRowView? value)
        {
            OnPropertyChanged(nameof(HasSelectedRow));
            UpdateRowDetails(value);
        }

        private void UpdateRowDetails(DataRowView? row)
        {
            RowDetails.Clear();

            if (row is null)
                return;

            foreach (DataColumn column in row.Row.Table.Columns)
            {
                var rawValue = row.Row[column];
                var displayValue = rawValue == DBNull.Value ? "NULL" : rawValue?.ToString();

                var columnName = column.ColumnName;
                var isPrimaryKey = _primaryKeyColumns.Contains(columnName);

                var displayColumnName = isPrimaryKey
                    ? $"{columnName} [PK]"
                    : columnName;

                RowDetails.Add(new RowDetailItem
                {
                    ColumnName = displayColumnName,
                    DisplayValue = displayValue,
                    IsPrimaryKey = isPrimaryKey
                });
            }
        }

        private async Task LoadPrimaryKeyColumnsAsync()
        {
            _primaryKeyColumns.Clear();

            if (string.IsNullOrWhiteSpace(ConnectionString) ||
                string.IsNullOrWhiteSpace(DatabaseName) ||
                string.IsNullOrWhiteSpace(TableName))
                return;

            var result = await _dataAccessService.GetPrimaryKeyColumnsAsync(ConnectionString, DatabaseName, TableName);

            foreach (var columnName in result.Data)
            {
                _primaryKeyColumns.Add(columnName);
            }
        }

        #endregion
    }

    public sealed class RowDetailItem
    {
        public required string ColumnName { get; init; }

        public string? DisplayValue { get; init; }

        public bool IsPrimaryKey { get; init; }
    }
}
