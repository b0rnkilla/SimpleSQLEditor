using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EfPlayground.Services;
using System.Collections.ObjectModel;

namespace EfPlayground.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region Fields

        private readonly SqlServerAdminService _sqlAdminService;

        #endregion

        #region Properties

        [ObservableProperty]
        private string _statusText = "Ready";

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
        public ObservableCollection<string> DataTypes { get; } = new()
        {
            "int",
            "nvarchar(50)",
            "nvarchar(100)",
            "datetime",
            "bit"
        };

        #endregion

        #region Constructor

        public MainViewModel(SqlServerAdminService sqlAdminService)
        {
            _sqlAdminService = sqlAdminService;
        }

        #endregion

        #region Methods & Events

        [RelayCommand]
        private async Task LoadDatabasesAsync()
        {
            try
            {
                StatusText = "Loading databases...";

                var databases = await _sqlAdminService.GetDatabasesAsync(ConnectionString);

                Databases.Clear();
                foreach (var db in databases)
                {
                    Databases.Add(db);
                }

                StatusText = $"Loaded {Databases.Count} databases.";
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
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

        #endregion
    }
}
