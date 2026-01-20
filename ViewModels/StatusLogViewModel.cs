using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSQLEditor.Infrastructure;
using System.Collections.ObjectModel;
using System.Windows;

namespace SimpleSQLEditor.ViewModels
{
    public partial class StatusLogViewModel : ObservableObject
    {
        #region Fields

        private readonly ObservableCollection<StatusEntry> _statusHistory;

        #endregion

        #region Properties

        public ObservableCollection<StatusEntry> StatusHistory => _statusHistory;

        [ObservableProperty]
        private StatusEntry? _selectedEntry;

        public IRelayCommand CopyCommand { get; }

        #endregion

        #region Constructor

        public StatusLogViewModel(ObservableCollection<StatusEntry> statusHistory)
        {
            _statusHistory = statusHistory;

            CopyCommand = new RelayCommand(CopySelected, CanCopySelected);
        }

        #endregion

        #region Methods & Events

        private bool CanCopySelected()
        {
            return SelectedEntry is not null;
        }

        private void CopySelected()
        {
            if (SelectedEntry is null)
                return;

            Clipboard.SetText($"[{SelectedEntry.Timestamp:HH:mm:ss}] {SelectedEntry.Message}");
        }

        partial void OnSelectedEntryChanged(StatusEntry? value)
        {
            CopyCommand.NotifyCanExecuteChanged();
        }

        #endregion
    }
}
