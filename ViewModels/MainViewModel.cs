using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EfPlayground.Infrastructure;
using EfPlayground.Services;
using EfPlayground.Views;
using System.Collections.ObjectModel;

namespace EfPlayground.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region Fields

        private readonly SqlServerAdminService _sqlAdminService;

        private readonly IWindowService _windowService;

        private readonly Dictionary<string, string> _columnDataTypes = new(StringComparer.OrdinalIgnoreCase);

        private bool _isAutoLoading;

        private const int StatusStepDelayMs = 1000;

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
        private string _connectionString =
            //"Server=.;Trusted_Connection=True;TrustServerCertificate=True;";
            "Server=C-OFFICE-CW\\SQLEXPRESS2022;Trusted_Connection=True;TrustServerCertificate=True;";

        [ObservableProperty]
        private string _selectedDatabase;

        [ObservableProperty]
        private string _selectedTable;

        [ObservableProperty]
        private string _selectedColumn;

        [ObservableProperty]
        private string _selectedDataType;

        [ObservableProperty]
        private string _columnValue;

        public ObservableCollection<string> Databases { get; } = new();
        public ObservableCollection<string> Tables { get; } = new();
        public ObservableCollection<string> Columns { get; } = new();
        public ObservableCollection<string> DataTypes { get; } = new(SqlDataTypes.Allowed);

        #endregion

        #region Constructor

        public MainViewModel(SqlServerAdminService sqlAdminService, IWindowService windowService)
        {
            _sqlAdminService = sqlAdminService;

            _windowService = windowService;

            _windowService.IsStatusLogOpenChanged += (_, isOpen) => IsStatusLogOpen = isOpen;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void OpenStatusLog()
        {
            _windowService.ShowStatusLog();
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            try
            {
                await SetStatusAsync(StatusLevel.Warning, "Connecting...");

                await _sqlAdminService.TestConnectionAsync(ConnectionString);

                IsConnected = true;
                await SetStatusAsync(StatusLevel.Success, "Ready");

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
                await SetStatusAsync(StatusLevel.Warning, "Loading databases...");

                var databases = await _sqlAdminService.GetDatabasesAsync(ConnectionString);

                Databases.Clear();
                foreach (var db in databases)
                {
                    Databases.Add(db);
                }

                await SetStatusAsync(StatusLevel.Success, $"Loaded {Databases.Count} databases.");
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
                StatusText = "Creating database...";

                await _sqlAdminService.CreateDatabaseAsync(ConnectionString, SelectedDatabase);

                StatusText = $"Database created: {SelectedDatabase}";
                await LoadDatabasesAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DeleteDatabaseAsync()
        {
            try
            {
                StatusText = "Deleting database...";

                await _sqlAdminService.DeleteDatabaseAsync(ConnectionString, SelectedDatabase);

                StatusText = $"Database deleted: {SelectedDatabase}";
                SelectedDatabase = string.Empty;

                await LoadDatabasesAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task LoadTablesAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase))
            {
                StatusText = "No database selected.";
                return;
            }

            try
            {
                StatusText = "Loading tables...";

                var tables = await _sqlAdminService.GetTablesAsync(
                    ConnectionString,
                    SelectedDatabase);

                Tables.Clear();
                foreach (var table in tables)
                {
                    Tables.Add(table);
                }

                StatusText = $"Loaded {Tables.Count} tables.";
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task CreateTableAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) ||
                string.IsNullOrWhiteSpace(SelectedTable))
            {
                StatusText = "Database or table name missing.";
                return;
            }

            try
            {
                StatusText = "Creating table...";

                await _sqlAdminService.CreateTableAsync(
                    ConnectionString,
                    SelectedDatabase,
                    SelectedTable);

                StatusText = $"Table created: {SelectedTable}";
                await LoadTablesAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DeleteTableAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) ||
                string.IsNullOrWhiteSpace(SelectedTable))
            {
                StatusText = "Database or table name missing.";
                return;
            }

            try
            {
                StatusText = "Deleting table...";

                await _sqlAdminService.DeleteTableAsync(
                    ConnectionString,
                    SelectedDatabase,
                    SelectedTable);

                StatusText = $"Table deleted: {SelectedTable}";
                SelectedTable = string.Empty;

                await LoadTablesAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task LoadColumnsAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) || string.IsNullOrWhiteSpace(SelectedTable))
            {
                StatusText = "No database or table selected.";
                return;
            }

            try
            {
                StatusText = "Loading columns...";

                var map = await _sqlAdminService.GetColumnDataTypesAsync(ConnectionString, SelectedDatabase, SelectedTable);

                _columnDataTypes.Clear();
                foreach (var kvp in map)
                {
                    _columnDataTypes[kvp.Key] = kvp.Value;
                }

                Columns.Clear();
                foreach (var columnName in _columnDataTypes.Keys)
                {
                    Columns.Add(columnName);
                }

                // NEU: Wenn bereits eine Column gewählt ist, Datentyp synchronisieren
                if (!string.IsNullOrWhiteSpace(SelectedColumn) && _columnDataTypes.TryGetValue(SelectedColumn, out var dataType))
                {
                    SelectedDataType = dataType;
                }

                StatusText = $"Loaded {Columns.Count} columns.";
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task CreateColumnAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) ||
                string.IsNullOrWhiteSpace(SelectedTable) ||
                string.IsNullOrWhiteSpace(SelectedColumn) ||
                string.IsNullOrWhiteSpace(SelectedDataType))
            {
                StatusText = "Database, table, column or data type missing.";
                return;
            }

            try
            {
                StatusText = "Creating column...";

                await _sqlAdminService.CreateColumnAsync(ConnectionString, SelectedDatabase, SelectedTable, SelectedColumn, SelectedDataType);

                StatusText = $"Column created: {SelectedColumn}";
                await LoadColumnsAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DeleteColumnAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedDatabase) ||
                string.IsNullOrWhiteSpace(SelectedTable) ||
                string.IsNullOrWhiteSpace(SelectedColumn))
            {
                StatusText = "Database, table or column name missing.";
                return;
            }

            try
            {
                StatusText = "Deleting column...";

                await _sqlAdminService.DeleteColumnAsync(ConnectionString, SelectedDatabase, SelectedTable, SelectedColumn);

                StatusText = $"Column deleted: {SelectedColumn}";
                SelectedColumn = string.Empty;

                await LoadColumnsAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
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

        partial void OnSelectedDatabaseChanged(string value)
        {
            if (_isAutoLoading)
                return;

            _ = HandleSelectedDatabaseChangedAsync(value);
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
                SelectedDataType = string.Empty;

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

            _ = HandleSelectedTableChangedAsync(value);
        }

        private async Task HandleSelectedTableChangedAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(SelectedDatabase))
                return;

            try
            {
                _isAutoLoading = true;

                SelectedColumn = string.Empty;
                SelectedDataType = string.Empty;

                Columns.Clear();

                await LoadColumnsAsync();
            }
            finally
            {
                _isAutoLoading = false;
            }
        }

        partial void OnSelectedColumnChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (_columnDataTypes.TryGetValue(value, out var dataType))
            {
                SelectedDataType = dataType;
            }
        }

        #endregion
    }
}
