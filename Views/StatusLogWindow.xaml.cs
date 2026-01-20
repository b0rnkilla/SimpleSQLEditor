using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;

namespace SimpleSQLEditor.Views
{
    public partial class StatusLogWindow : Window
    {
        #region Properties

        public ICommand CopyCommand { get; }

        #endregion

        #region Constructor

        public StatusLogWindow()
        {
            InitializeComponent();

            CopyCommand = new RelayCommand(CopySelected);

            Loaded += (_, _) => ScrollToLast();

            DataContextChanged += (_, _) =>
            {
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.StatusHistory.CollectionChanged += (_, _) => ScrollToLast();
                }
            };
        }

        #endregion

        #region Methods & Events

        private void ScrollToLast()
        {
            if (LogListBox.Items.Count > 0)
            {
                var lastItem = LogListBox.Items[LogListBox.Items.Count - 1];
                LogListBox.ScrollIntoView(lastItem);
            }
        }

        private void CopySelected()
        {
            if (LogListBox.SelectedItem is Infrastructure.StatusEntry entry)
            {
                Clipboard.SetText($"[{entry.Timestamp:HH:mm:ss}] {entry.Message}");
            }
        }

        private void CopySelected_OnClick(object sender, RoutedEventArgs e)
        {
            CopySelected();
        }

        #endregion
    }
}