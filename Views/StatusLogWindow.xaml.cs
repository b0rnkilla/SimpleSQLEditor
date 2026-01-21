using System.Collections.Specialized;
using System.Windows;
using System.Windows.Threading;

namespace SimpleSQLEditor.Views
{
    public partial class StatusLogWindow : Window
    {
        #region Fields

        private INotifyCollectionChanged? _currentCollection;

        #endregion

        #region Constructor

        public StatusLogWindow()
        {
            InitializeComponent();

            Loaded += (_, _) => ScrollToLast();

            DataContextChanged += (_, _) =>
            {
                if (_currentCollection != null)
                {
                    _currentCollection.CollectionChanged -= StatusHistory_CollectionChanged;
                    _currentCollection = null;
                }

                if (DataContext is ViewModels.StatusLogViewModel vm)
                {
                    _currentCollection = vm.StatusHistory;
                    _currentCollection.CollectionChanged += StatusHistory_CollectionChanged;
                }
            };

            Closed += (_, _) =>
            {
                if (_currentCollection != null)
                {
                    _currentCollection.CollectionChanged -= StatusHistory_CollectionChanged;
                    _currentCollection = null;
                }
            };
        }

        #endregion

        #region Methods & Events

        private void StatusHistory_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ScrollToLast();
        }

        private void ScrollToLast()
        {
            if (LogListBox.Items.Count == 0)
                return;

            var lastItem = LogListBox.Items[LogListBox.Items.Count - 1];

            Dispatcher.BeginInvoke(new Action(() =>
            {
                LogListBox.ScrollIntoView(lastItem);
            }), DispatcherPriority.Background);
        }

        #endregion
    }
}
