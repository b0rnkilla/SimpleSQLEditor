using System.Windows;
using SimpleSQLEditor.ViewModels;

namespace SimpleSQLEditor
{
    public partial class MainWindow : HandyControl.Controls.Window
    {
        #region Constructor

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        #endregion
    }
}
