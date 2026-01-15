using CommunityToolkit.Mvvm.ComponentModel;

namespace EfPlayground.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region Properties

        [ObservableProperty]
        private string _statusText = "Ready";

        #endregion

        #region Constructor

        public MainViewModel()
        {
        }

        #endregion
    }
}
