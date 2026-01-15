using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace EfPlayground.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region Properties

        [ObservableProperty]
        private string _connectionString;

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
    }
}
