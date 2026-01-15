using System.Windows;
using EfPlayground.ViewModels;

namespace EfPlayground
{
    public partial class MainWindow : Window
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
