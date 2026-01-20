using System.Collections.Specialized;
using System.Windows;

namespace SimpleSQLEditor.Views
{
    public partial class StatusLogWindow : Window
    {
        #region Constructor

        public StatusLogWindow()
        {
            InitializeComponent();

            Loaded += (_, _) => ScrollToLast();

            DataContextChanged += (_, _) =>
            {
                if (DataContext is ViewModels.StatusLogViewModel vm)
                {
                    vm.StatusHistory.CollectionChanged -= StatusHistory_CollectionChanged;
                    vm.StatusHistory.CollectionChanged += StatusHistory_CollectionChanged;
                }
            };

            Closed += (_, _) =>
            {
                if (DataContext is ViewModels.StatusLogViewModel vm)
                {
                    vm.StatusHistory.CollectionChanged -= StatusHistory_CollectionChanged;
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
            if (LogListBox.Items.Count > 0)
            {
                var lastItem = LogListBox.Items[LogListBox.Items.Count - 1];
                LogListBox.ScrollIntoView(lastItem);
            }
        }

        #endregion
    }
}
