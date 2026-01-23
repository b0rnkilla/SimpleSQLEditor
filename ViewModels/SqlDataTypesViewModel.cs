using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSQLEditor.Infrastructure;
using SimpleSQLEditor.Services.Ui;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SimpleSQLEditor.ViewModels
{
    public partial class SqlDataTypesViewModel : ObservableObject
    {
        #region Fields

        private readonly IColumnDefinitionService _columnDefinitionService;

        #endregion

        #region Properties

        public ObservableCollection<string> AllowedDataTypes { get; } = new(SqlDataTypes.Allowed);

        [ObservableProperty]
        private string? _selectedDataType;

        public ICommand AddToColumnDefinitionCommand { get; }

        public ICommand CopyCommand { get; }

        #endregion

        #region Constructor

        public SqlDataTypesViewModel(IColumnDefinitionService columnDefinitionService)
        {
            _columnDefinitionService = columnDefinitionService;

            CopyCommand = new RelayCommand(CopySelected, CanCopySelected);
            AddToColumnDefinitionCommand = new RelayCommand(AddToColumnDefinition, CanCopySelected);
        }

        #endregion

        #region Methods & Events

        private void AddToColumnDefinition()
        {
            if (string.IsNullOrWhiteSpace(SelectedDataType))
                return;

            _columnDefinitionService.RequestInsertDataType(SelectedDataType);
        }

        private bool CanCopySelected()
        {
            return !string.IsNullOrWhiteSpace(SelectedDataType);
        }

        private void CopySelected()
        {
            if (string.IsNullOrWhiteSpace(SelectedDataType))
                return;

            Clipboard.SetText(SelectedDataType);
        }

        partial void OnSelectedDataTypeChanged(string? value)
        {
            if (CopyCommand is RelayCommand copy)
                copy.NotifyCanExecuteChanged();

            if (AddToColumnDefinitionCommand is RelayCommand add)
                add.NotifyCanExecuteChanged();
        }

        #endregion
    }
}
