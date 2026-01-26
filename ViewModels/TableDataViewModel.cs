using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSQLEditor.Services.DataAccess;
using SimpleSQLEditor.Services.EfCore;
using System.Collections.ObjectModel;
using System.Data;

namespace SimpleSQLEditor.ViewModels
{
    public partial class TableDataViewModel : ObservableObject
    {
        #region Fields

        private readonly IDataAccessService _dataAccessService;

        private readonly HashSet<string> _primaryKeyColumns = new(StringComparer.OrdinalIgnoreCase);

        private readonly EfDatabaseAdminService _efDatabaseAdminService;

        private EfDatabaseAdminService.EfRowTrackingSession? _trackingSession;

        private const string DEFAULT_TRACKING_DEMO_COLUMN = "Id";

        private const int HEADER_STATUS_DELAY_MS = 500;

        private int _headerStatusSequence;

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
        private string _headerStatusText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorText;

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorText);

        [ObservableProperty]
        private DataRowView? _selectedRow;

        public ObservableCollection<RowDetailItem> RowDetails { get; } = new();

        public bool HasSelectedRow => SelectedRow is not null;

        [ObservableProperty]
        private bool _hasPrimaryKey;

        [ObservableProperty]
        private string _trackingAvailabilityText = string.Empty;

        [ObservableProperty]
        private string _efTrackingStateText = string.Empty;

        public bool CanRunTrackingDemo => _trackingSession is not null;

        #endregion

        #region Commands

        public IAsyncRelayCommand ReloadCommand { get; }

        public IRelayCommand SimulateChangeCommand { get; }

        public IRelayCommand RevertChangeCommand { get; }

        #endregion

        #region Constructor

        public TableDataViewModel(IDataAccessService dataAccessService, EfDatabaseAdminService efDatabaseAdminService)
        {
            _dataAccessService = dataAccessService;
            _efDatabaseAdminService = efDatabaseAdminService;

            _connectionString = string.Empty;
            _databaseName = string.Empty;
            _tableName = string.Empty;

            ReloadCommand = new AsyncRelayCommand(LoadAsync, CanReload);

            SimulateChangeCommand = new RelayCommand(SimulateChange, () => CanRunTrackingDemo);
            RevertChangeCommand = new RelayCommand(RevertChange, () => CanRunTrackingDemo);
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

            ResetTracking();

            var loadingStartedAt = DateTime.UtcNow;

            try
            {
                IsLoading = true;
                HeaderStatusText = "Loading...";
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

                var elapsedMs = (int)(DateTime.UtcNow - loadingStartedAt).TotalMilliseconds;
                var remainingMs = HEADER_STATUS_DELAY_MS - elapsedMs;

                if (remainingMs > 0)
                    await Task.Delay(remainingMs);

                if (_dataAccessService.ProviderName.Equals("EF", StringComparison.OrdinalIgnoreCase))
                    HeaderStatusText = TrackingAvailabilityText;
                else
                    HeaderStatusText = string.Empty;

                RowsLoaded?.Invoke(this, TableData?.Count ?? 0);
            }
            catch (Exception ex)
            {
                TableData = null;
                SelectedRow = null;
                UpdateRowDetails(null);

                ErrorText = ex.Message;
                ResetTracking();

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

            _ = TryStartEfTrackingAsync(value);

            OnPropertyChanged(nameof(CanRunTrackingDemo));
        }

        private async Task TryStartEfTrackingAsync(DataRowView? row)
        {
            ResetTracking();

            if (row is null)
                return;

            if (!_dataAccessService.ProviderName.Equals("EF", StringComparison.OrdinalIgnoreCase))
                return;

            if (!HasPrimaryKey)
                return;

            if (_primaryKeyColumns.Count != 1)
                return;

            var pkColumn = _primaryKeyColumns.First();

            if (!row.Row.Table.Columns.Contains(pkColumn))
                return;

            var pkValue = row.Row[pkColumn];
            if (pkValue == DBNull.Value)
                return;

            try
            {
                _trackingSession = await _efDatabaseAdminService.StartRowTrackingAsync(
                    ConnectionString,
                    DatabaseName,
                    TableName,
                    pkColumn,
                    pkValue,
                    row.Row.Table);

                var snapshot = _trackingSession.GetSnapshot();

                EfTrackingStateText = $"EF Tracking State: {snapshot.State}";

                OnPropertyChanged(nameof(CanRunTrackingDemo));
                SimulateChangeCommand.NotifyCanExecuteChanged();
                RevertChangeCommand.NotifyCanExecuteChanged();

                var messages = new List<string>();

                if (!string.IsNullOrWhiteSpace(TrackingAvailabilityText))
                    messages.Add(TrackingAvailabilityText);

                if (!string.IsNullOrWhiteSpace(EfTrackingStateText))
                    messages.Add(EfTrackingStateText);

                await SetHeaderStatusSequenceAsync(EfTrackingStateText);
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                OnPropertyChanged(nameof(HasError));
            }
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

            HasPrimaryKey = false;
            TrackingAvailabilityText = string.Empty;

            if (string.IsNullOrWhiteSpace(ConnectionString) ||
                string.IsNullOrWhiteSpace(DatabaseName) ||
                string.IsNullOrWhiteSpace(TableName))
                return;

            var result = await _dataAccessService.GetPrimaryKeyColumnsAsync(ConnectionString, DatabaseName, TableName);

            foreach (var columnName in result.Data)
            {
                _primaryKeyColumns.Add(columnName);
            }

            HasPrimaryKey = _primaryKeyColumns.Count > 0;

            if (!HasPrimaryKey)
            {
                TrackingAvailabilityText = "Kein Primary Key gefunden. EF-Tracking-Demo ist für diese Tabelle deaktiviert.";
                return;
            }

            if (_primaryKeyColumns.Count > 1)
            {
                TrackingAvailabilityText = "Composite Primary Key erkannt. EF-Tracking-Demo wird später bewusst separat behandelt.";
                return;
            }

            TrackingAvailabilityText = "Primary Key erkannt. EF-Tracking-Demo ist für diese Tabelle möglich.";
        }

        private void ResetTracking()
        {
            _trackingSession?.Dispose();
            _trackingSession = null;
            EfTrackingStateText = string.Empty;

            OnPropertyChanged(nameof(CanRunTrackingDemo));
            SimulateChangeCommand.NotifyCanExecuteChanged();
            RevertChangeCommand.NotifyCanExecuteChanged();

            if (!_dataAccessService.ProviderName.Equals("EF", StringComparison.OrdinalIgnoreCase))
                HeaderStatusText = string.Empty;
            else
                HeaderStatusText = TrackingAvailabilityText;
        }

        private async Task SetHeaderStatusSequenceAsync(params string[] messages)
        {
            var sequenceId = Interlocked.Increment(ref _headerStatusSequence);

            foreach (var message in messages)
            {
                if (sequenceId != _headerStatusSequence)
                    return;

                HeaderStatusText = message;

                if (string.IsNullOrWhiteSpace(message))
                    continue;

                await Task.Delay(HEADER_STATUS_DELAY_MS);
            }
        }

        private void SimulateChange()
        {
            if (_trackingSession is null || SelectedRow is null)
                return;

            var columnName = GetDemoColumnName();
            if (string.IsNullOrWhiteSpace(columnName))
                return;

            var currentValue = SelectedRow.Row.Table.Columns.Contains(columnName)
                ? SelectedRow.Row[columnName]
                : null;

            var newValue = BuildDemoValue(currentValue);

            _trackingSession.SetValue(columnName, newValue);

            var snapshot = _trackingSession.GetSnapshot();

            EfTrackingStateText = BuildTrackingStateText(snapshot);
            HeaderStatusText = EfTrackingStateText;
        }

        private void RevertChange()
        {
            if (_trackingSession is null)
                return;

            _trackingSession.RevertChanges();

            var snapshot = _trackingSession.GetSnapshot();

            EfTrackingStateText = BuildTrackingStateText(snapshot);
            HeaderStatusText = EfTrackingStateText;
        }

        private string GetDemoColumnName()
        {
            if (SelectedRow is null)
                return string.Empty;

            var columns = SelectedRow.Row.Table.Columns.Cast<DataColumn>().ToList();

            var pkColumn = _primaryKeyColumns.Count == 1
                ? _primaryKeyColumns.First()
                : null;

            var candidate = columns
                .Select(c => c.ColumnName)
                .FirstOrDefault(name => !string.Equals(name, pkColumn, StringComparison.OrdinalIgnoreCase));

            return candidate ?? pkColumn ?? string.Empty;
        }

        private static object BuildDemoValue(object? currentValue)
        {
            if (currentValue is null || currentValue == DBNull.Value)
                return "DemoValue";

            if (currentValue is int i)
                return i + 1;

            if (currentValue is long l)
                return l + 1;

            if (currentValue is string s)
                return $"{s}_demo";

            if (currentValue is DateTime dt)
                return dt.AddSeconds(1);

            return "DemoValue";
        }

        private static string BuildTrackingStateText(EfDatabaseAdminService.EfTrackingSnapshot snapshot)
        {
            if (snapshot.ModifiedColumns.Count == 0)
                return $"EF Tracking State: {snapshot.State}";

            return $"EF Tracking State: {snapshot.State} (Modified: {string.Join(", ", snapshot.ModifiedColumns)})";
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
