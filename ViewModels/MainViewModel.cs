using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleSQLEditor.Infrastructure;
using SimpleSQLEditor.Services;
using System.Collections.ObjectModel;

namespace SimpleSQLEditor.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region Fields

        private readonly SqlServerAdminService _sqlAdminService;

        private readonly IConfiguration _configuration;

        private readonly IWindowService _windowService;

        private readonly IDialogService _dialogService;

        private readonly IColumnDefinitionService _columnDefinitionService;

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
        private string _connectionString = string.Empty;

        [ObservableProperty]
        private string _selectedDatabase;

        [ObservableProperty]
        private string _selectedTable;

        [ObservableProperty]
        private string _selectedColumn;

        [ObservableProperty]
        private bool _isStatusLogOpen;

        [ObservableProperty]
        private bool _isTableDataOpen;

        [ObservableProperty]
        private bool _isSqlDataTypesOpen;

        public ObservableCollection<string> Databases { get; } = new();
        public ObservableCollection<string> Tables { get; } = new();
        public ObservableCollection<string> Columns { get; } = new();

        #endregion

        #region Constructor

        public MainViewModel(SqlServerAdminService sqlAdminService, IConfiguration configuration, IWindowService windowService, IDialogService dialogService, IColumnDefinitionService columnDefinitionService)
        {
            _sqlAdminService = sqlAdminService;
            _configuration = configuration;
            _windowService = windowService;
            _dialogService = dialogService;
            _columnDefinitionService = columnDefinitionService;

            ConnectionString = _configuration.GetConnectionString("SqlServer") ?? string.Empty;

            _columnDefinitionService.DataTypeInsertRequested += async (_, dataType) =>
            {
                var updated = ApplyDataTypeToColumnDefinition(SelectedColumn, dataType);

                if (string.IsNullOrWhiteSpace(updated))
                {
                    await SetStatusAsync(StatusLevel.Warning, "Enter a column name first.");
                    return;
                }

                SelectedColumn = updated;
            };
        }

        #endregion

        #region Commands

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

                var created = await _sqlAdminService.CreateDatabaseAsync(ConnectionString, SelectedDatabase);

                if (created)
                    await SetStatusAsync(StatusLevel.Success, $"Database created: {SelectedDatabase}");
                else
                    await SetStatusAsync(StatusLevel.Warning, $"Database already exists: {SelectedDatabase}");

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
                await SetStatusAsync(StatusLevel.Info, "Delete cancelled.");
                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Deleting database...");

                var deleted = await _sqlAdminService.DeleteDatabaseAsync(ConnectionString, SelectedDatabase);

                if (deleted)
                    await SetStatusAsync(StatusLevel.Success, $"Database deleted: {SelectedDatabase}");
                else
                    await SetStatusAsync(StatusLevel.Warning, $"Database '{SelectedDatabase}' does not exist.");

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

                var tables = await _sqlAdminService.GetTablesAsync( ConnectionString, SelectedDatabase);

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
            if (string.IsNullOrWhiteSpace(SelectedDatabase) || string.IsNullOrWhiteSpace(SelectedTable))
            {
                await SetStatusAsync(StatusLevel.Warning, "Database or table name missing.");
                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Creating table...");

                var created = await _sqlAdminService.CreateTableAsync(ConnectionString, SelectedDatabase, SelectedTable);

                if (created)
                    await SetStatusAsync(StatusLevel.Success, $"Table created: {SelectedTable}");
                else
                    await SetStatusAsync(StatusLevel.Warning, $"Table already exists: {SelectedTable}");

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
            if (string.IsNullOrWhiteSpace(SelectedDatabase) || string.IsNullOrWhiteSpace(SelectedTable))
            {
                await SetStatusAsync(StatusLevel.Warning, "Database or table name missing.");
                return;
            }

            var shouldDelete = _dialogService.Confirm(
                "Confirm delete",
                $"Do you really want to delete table '{SelectedTable}'?");

            if (!shouldDelete)
            {
                await SetStatusAsync(StatusLevel.Info, "Delete cancelled.");
                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Deleting table...");

                var deleted = await _sqlAdminService.DeleteTableAsync(ConnectionString,  SelectedDatabase, SelectedTable);

                if (deleted)
                    await SetStatusAsync(StatusLevel.Success, $"Table deleted: {SelectedTable}");
                else
                    await SetStatusAsync(StatusLevel.Warning, $"Table '{SelectedTable}' does not exist.");

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
                await SetStatusAsync(StatusLevel.Warning, "No database or table selected.");
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
            if (string.IsNullOrWhiteSpace(SelectedDatabase) || string.IsNullOrWhiteSpace(SelectedTable) || string.IsNullOrWhiteSpace(SelectedColumn))
            {
                await SetStatusAsync(StatusLevel.Error, "Database, table or column definition missing.", withDelay: false);
                return;
            }

            if (!TryParseColumnDisplay(SelectedColumn, out var columnName, out var dataType) || string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(dataType))
            {
                await SetStatusAsync(StatusLevel.Error, "Column must be in format: ColumnName (DataType)", withDelay: false);
                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Creating column...");

                var created = await _sqlAdminService.CreateColumnAsync(ConnectionString, SelectedDatabase, SelectedTable, columnName, dataType);

                if (created)
                    await SetStatusAsync(StatusLevel.Success, $"Column created: {columnName}");
                else
                    await SetStatusAsync(StatusLevel.Warning, $"Column already exists: {columnName}");

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
            if (string.IsNullOrWhiteSpace(SelectedDatabase) || string.IsNullOrWhiteSpace(SelectedTable) || string.IsNullOrWhiteSpace(SelectedColumn))
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
                await SetStatusAsync(StatusLevel.Info, "Delete cancelled.");
                return;
            }

            try
            {
                await SetStatusAsync(StatusLevel.Info, "Deleting column...");

                var deleted = await _sqlAdminService.DeleteColumnAsync(ConnectionString, SelectedDatabase, SelectedTable, columnName);

                if (deleted)
                    await SetStatusAsync(StatusLevel.Success, $"Column deleted: {columnName}");
                else
                    await SetStatusAsync(StatusLevel.Warning, $"Column '{columnName}' does not exist.");

                SelectedColumn = string.Empty;

                await LoadColumnsAsync();
            }
            catch (Exception ex)
            {
                await SetStatusAsync(StatusLevel.Error, $"Error: {ex.Message}", withDelay: false);
            }
        }

        [RelayCommand]
        private void OpenStatusLog()
        {
            var statusLogViewModel = new StatusLogViewModel(StatusHistory);

            _windowService.ShowWindow<Views.StatusLogWindow>(
                statusLogViewModel,
                isOpen => IsStatusLogOpen = isOpen);
        }

        [RelayCommand]
        private async Task OpenTableDataAsync()
        {
            if (!IsConnected)
            {
                await SetStatusAsync(StatusLevel.Warning, "Not connected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedDatabase) || string.IsNullOrWhiteSpace(SelectedTable))
            {
                await SetStatusAsync(StatusLevel.Warning, "No database or table selected.");
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

        [RelayCommand]
        private void OpenSqlDataTypes()
        {
            var viewModel = new SqlDataTypesViewModel(_columnDefinitionService);

            _windowService.ShowWindow<Views.SqlDataTypesWindow>(
                viewModel,
                isOpen => IsSqlDataTypesOpen = isOpen);
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

            var openIndex = input.IndexOf('(');
            var closeIndex = input.LastIndexOf(')');

            if (openIndex > 0 && closeIndex > openIndex && closeIndex == input.Length - 1)
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

        private static string ApplyDataTypeToColumnDefinition(string? current, string dataType)
        {
            var text = (current ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var openIndex = text.IndexOf('(');
            if (openIndex >= 0)
                text = text[..openIndex].TrimEnd();

            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return $"{text} ({dataType})";
        }

        #endregion
    }
}

#region TODO – Roadmap / Lernziele

// TODO: Column Constraints
// - Allow setting NULL / NOT NULL on column creation
// - Show NULL / NOT NULL status in column display
// - Change NULLability of existing columns (ALTER TABLE)

// TODO: Primary Keys
// - Display PK columns (done read-only in v0.7.7)
// - Allow setting PK on column creation
// - Allow removing / changing PK (single vs. composite keys)
// - Handle identity vs. non-identity PKs

// TODO: Foreign Keys
// - Display FK relationships (done read-only in v0.7.7)
// - Add FK to existing column
// - Select referenced table and column
// - Configure ON DELETE / ON UPDATE behavior

// TODO: Table Data (EF Core)
// - Insert new rows (Create)
// - Edit existing rows (Update)
// - Delete rows
// - Handle validation errors
// - Understand EF Change Tracking

// TODO: EF Core Integration
// - Introduce DbContext (no Code-First, no migrations)
// - Dynamic table access without fixed entities
// - Compare raw SQL vs EF queries
// - Observe generated SQL

#endregion
