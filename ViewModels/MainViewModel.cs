using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSQLEditor.Infrastructure;
using SimpleSQLEditor.Services;
using System.Collections.ObjectModel;

namespace SimpleSQLEditor.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region Fields

        private readonly SqlServerAdminService _sqlAdminService;

        private readonly IWindowService _windowService;

        private readonly IDialogService _dialogService;

        private readonly Dictionary<string, string> _columnDataTypes = new(StringComparer.OrdinalIgnoreCase);

        private bool _isAutoLoading;

        private const int StatusStepDelayMs = 500;

        #endregion

        #region Properties

        [ObservableProperty]
        private bool _isConnected;

        [ObservableProperty]
        private string _statusText;

        [ObservableProperty]
        private StatusLevel _statusLevel = StatusLevel.Info;

        public ObservableCollection<StatusEntry> StatusHistory { get; } = new();

        [ObservableProperty]
        private bool _isStatusLogOpen;

        [ObservableProperty]
        private bool _isTableDataOpen;

        [ObservableProperty]
        private string _connectionString =
            //"Server=.;Trusted_Connection=True;TrustServerCertificate=True;";
            "Server=C-OFFICE-CW\\SQLEXPRESS2022;Trusted_Connection=True;TrustServerCertificate=True;";

        [ObservableProperty]
        private string _selectedDatabase;

        [ObservableProperty]
        private string _selectedTable;

        [ObservableProperty]
        private string _selectedColumn;

        public ObservableCollection<string> Databases { get; } = new();
        public ObservableCollection<string> Tables { get; } = new();
        public ObservableCollection<string> Columns { get; } = new();

        #endregion

        #region Constructor

        public MainViewModel(SqlServerAdminService sqlAdminService, IWindowService windowService, IDialogService dialogService)
        {
            _sqlAdminService = sqlAdminService;
            _windowService = windowService;
            _dialogService = dialogService;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void OpenStatusLog()
        {
            var statusLogViewModel = new StatusLogViewModel(StatusHistory);

            _windowService.ShowWindow<Views.StatusLogWindow>(
                statusLogViewModel,
                isOpen => IsStatusLogOpen = isOpen);
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            try
            {
                await SetStatusAsync(StatusLevel.Warning, "Connecting...");

                await _sqlAdminService.TestConnectionAsync(ConnectionString);

                IsConnected = true;

                await SetStatusAsync(StatusLevel.Success, "Connection established.");

                await LoadDatabasesAsync();
            }
            catch (Exception ex)
            {
                IsConnected = false;
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task LoadDatabasesAsync()
        {
            try
            {
                await SetStatusAsync(StatusLevel.Info, "Loading databases...");

                var databases = await _sqlAdminService.GetDatabasesAsync(ConnectionString);

                Databases.Clear();

                foreach (var db in databases)
                {
                    Databases.Add(db);
                }

                await SetStatusAsync(StatusLevel.Info, $"Loaded {Databases.Count} databases.");
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task CreateDatabaseAsync()
        {
            try
            {
                await SetStatusAsync(StatusLevel.Info, "Creating database...");

                await _sqlAdminService.CreateDatabaseAsync(ConnectionString, SelectedDatabase);

                await SetStatusAsync(StatusLevel.Info, $"Database created: {SelectedDatabase}");

                await LoadDatabasesAsync();
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task DeleteDatabaseAsync()
        {
            var shouldDelete = _dialogService.Confirm(
                "Confirm delete",
                $"Do you really want to delete database '{SelectedDatabase}'?");

            if (!shouldDelete)
            {
                await SetStatusAsync(StatusLevel.Info, "Delete cancelled.", withDelay: false);
                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Deleting database...");

                await _sqlAdminService.DeleteDatabaseAsync(ConnectionString, SelectedDatabase);

                await SetStatusAsync(StatusLevel.Info, $"Database deleted: {SelectedDatabase}");

                SelectedDatabase = string.Empty;

                await LoadDatabasesAsync();
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task LoadTablesAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase))
            {
                await SetStatusAsync(StatusLevel.Warning, "No database selected.");

                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Loading tables...");

                var tables = await _sqlAdminService.GetTablesAsync(
                    ConnectionString,
                    SelectedDatabase);

                Tables.Clear();

                foreach (var table in tables)
                {
                    Tables.Add(table);
                }

                await SetStatusAsync(StatusLevel.Info, $"Loaded {Tables.Count} tables.");
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task CreateTableAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) ||
                string.IsNullOrWhiteSpace(SelectedTable))
            {
                await SetStatusAsync(StatusLevel.Warning, "Database or table name missing.", withDelay: false);

                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Creating table...");

                await _sqlAdminService.CreateTableAsync(
                    ConnectionString,
                    SelectedDatabase,
                    SelectedTable);

                await SetStatusAsync(StatusLevel.Info, $"Table created: {SelectedTable}");

                await LoadTablesAsync();
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task DeleteTableAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) ||
                string.IsNullOrWhiteSpace(SelectedTable))
            {
                await SetStatusAsync(StatusLevel.Warning, "Database or table name missing.", withDelay: false);

                return;
            }

            var shouldDelete = _dialogService.Confirm(
                "Confirm delete",
                $"Do you really want to delete table '{SelectedTable}'?");

            if (!shouldDelete)
            {
                await SetStatusAsync(StatusLevel.Info, "Delete cancelled.", withDelay: false);
                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Deleting table...");

                await _sqlAdminService.DeleteTableAsync(
                    ConnectionString,
                    SelectedDatabase,
                    SelectedTable);

                await SetStatusAsync(StatusLevel.Info, $"Table deleted: {SelectedTable}");

                SelectedTable = string.Empty;

                await LoadTablesAsync();
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task LoadColumnsAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) || string.IsNullOrWhiteSpace(SelectedTable))
            {
                await SetStatusAsync(StatusLevel.Warning, "No database or table selected.", withDelay: false);

                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Loading columns...");

                var map = await _sqlAdminService.GetColumnDataTypesAsync(ConnectionString, SelectedDatabase, SelectedTable);

                _columnDataTypes.Clear();
                foreach (var kvp in map)
                {
                    _columnDataTypes[kvp.Key] = kvp.Value;
                }

                Columns.Clear();
                foreach (var kvp in _columnDataTypes)
                {
                    Columns.Add($"{kvp.Key} ({kvp.Value})");
                }

                await SetStatusAsync(StatusLevel.Info, $"Loaded {Columns.Count} columns.");
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task CreateColumnAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) ||
                string.IsNullOrWhiteSpace(SelectedTable) ||
                string.IsNullOrWhiteSpace(SelectedColumn))
            {
                await SetStatusAsync(StatusLevel.Error, "Database, table or column definition missing.", withDelay: false);
                return;
            }

            if (!TryParseColumnDisplay(SelectedColumn, out var columnName, out var dataType) ||
                string.IsNullOrWhiteSpace(columnName) ||
                string.IsNullOrWhiteSpace(dataType))
            {
                await SetStatusAsync(StatusLevel.Error, "Column must be in format: ColumnName (DataType)", withDelay: false);
                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Creating column...");

                await _sqlAdminService.CreateColumnAsync(ConnectionString, SelectedDatabase, SelectedTable, columnName, dataType);

                await SetStatusAsync(StatusLevel.Info, $"Column created: {columnName}");

                await LoadColumnsAsync();
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task DeleteColumnAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) ||
                string.IsNullOrWhiteSpace(SelectedTable) ||
                string.IsNullOrWhiteSpace(SelectedColumn))
            {
                await SetStatusAsync(StatusLevel.Error, "Database, table or column name missing.", withDelay: false);
                return;
            }

            if (!TryParseColumnDisplay(SelectedColumn, out var columnName, out _) || string.IsNullOrWhiteSpace(columnName))
            {
                await SetStatusAsync(StatusLevel.Error, "Invalid column selection.", withDelay: false);
                return;
            }

            var shouldDelete = _dialogService.Confirm(
                "Confirm delete",
                $"Do you really want to delete column '{columnName}'?");

            if (!shouldDelete)
            {
                await SetStatusAsync(StatusLevel.Info, "Delete cancelled.", withDelay: false);
                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Deleting column...");

                await _sqlAdminService.DeleteColumnAsync(ConnectionString, SelectedDatabase, SelectedTable, columnName);

                await SetStatusAsync(StatusLevel.Info, $"Column deleted: {columnName}");

                SelectedColumn = string.Empty;

                await LoadColumnsAsync();
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private async Task OpenTableDataAsync()
        {
            if (!IsConnected)
            {
                await SetStatusAsync(StatusLevel.Warning, "Not connected.", withDelay: false);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedDatabase) || string.IsNullOrWhiteSpace(SelectedTable))
            {
                await SetStatusAsync(StatusLevel.Warning, "No database or table selected.", withDelay: false);
                return;
            }

            try
            {
                var tableDataViewModel = new TableDataViewModel(
                    _sqlAdminService,
                    ConnectionString,
                    SelectedDatabase,
                    SelectedTable,
                    maxRows: 100);

                _windowService.ShowWindow<Views.TableDataWindow>(
                    tableDataViewModel,
                    isOpen => IsTableDataOpen = isOpen);

                await tableDataViewModel.LoadAsync();
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        #endregion

        #region Methods & Events

        private async Task SetStatusAsync(StatusLevel level, string message, bool withDelay = true)
        {
            StatusLevel = level;
            StatusText = message;

            StatusHistory.Add(new StatusEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            });

            if (withDelay)
            {
                await Task.Delay(StatusStepDelayMs);
            }
        }

        private static bool TryParseColumnDisplay(string input, out string columnName, out string? dataType)
        {
            columnName = string.Empty;
            dataType = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            var openIndex = input.LastIndexOf('(');
            var closeIndex = input.LastIndexOf(')');

            if (openIndex > 0 && closeIndex > openIndex)
            {
                columnName = input[..openIndex].Trim();
                dataType = input[(openIndex + 1)..closeIndex].Trim();
                return !string.IsNullOrWhiteSpace(columnName);
            }

            // Fallback: Nutzer gibt nur "ColumnName" ein
            columnName = input;
            return true;
        }

        partial void OnSelectedDatabaseChanged(string value)
        {
            if (_isAutoLoading)
                return;

            FireAndForget(() => HandleSelectedDatabaseChangedAsync(value));
        }

        private async Task HandleSelectedDatabaseChangedAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            try
            {
                _isAutoLoading = true;

                SelectedTable = string.Empty;
                SelectedColumn = string.Empty;

                Tables.Clear();
                Columns.Clear();

                await LoadTablesAsync();
            }
            finally
            {
                _isAutoLoading = false;
            }
        }

        partial void OnSelectedTableChanged(string value)
        {
            if (_isAutoLoading)
                return;

            FireAndForget(() => HandleSelectedTableChangedAsync(value));
        }

        private async Task HandleSelectedTableChangedAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(SelectedDatabase))
                return;

            try
            {
                _isAutoLoading = true;

                SelectedColumn = string.Empty;

                Columns.Clear();

                await LoadColumnsAsync();
            }
            finally
            {
                _isAutoLoading = false;
            }
        }

        private void FireAndForget(Func<Task> asyncAction)
        {
            _ = FireAndForgetInternalAsync(asyncAction);
        }

        private async Task FireAndForgetInternalAsync(Func<Task> asyncAction)
        {
            try
            {
                await asyncAction();
            }
            catch (Exception ex)
            {
                await SetStatusAsync(
                    StatusLevel.Error,
                    $"Background error: {ex.Message}",
                    withDelay: false);
            }
        }

        #endregion
    }
}