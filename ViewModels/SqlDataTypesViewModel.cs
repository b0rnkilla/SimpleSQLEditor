using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSQLEditor.Infrastructure;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SimpleSQLEditor.ViewModels
{
    public partial class SqlDataTypesViewModel : ObservableObject
    {
        #region Properties

        public ObservableCollection<string> AllowedDataTypes { get; } = new(SqlDataTypes.Allowed);

        [ObservableProperty]
        private string? _selectedDataType;

        public ICommand CopyCommand { get; }

        #endregion

        #region Constructor

        public SqlDataTypesViewModel()
        {
            CopyCommand = new RelayCommand(CopySelected, CanCopySelected);
        }

        #endregion

        #region Methods & Events

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
            if (CopyCommand is RelayCommand rc)
                rc.NotifyCanExecuteChanged();
        }

        #endregion
    }
}
