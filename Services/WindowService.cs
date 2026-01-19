using EfPlayground.ViewModels;
using EfPlayground.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EfPlayground.Services
{
    public class WindowService : IWindowService
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;
        private StatusLogWindow? _statusLogWindow;

        #endregion

        #region Constructor

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods & Events

        public void ShowStatusLog()
        {
            _statusLogWindow ??= _serviceProvider.GetRequiredService<StatusLogWindow>();

            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            _statusLogWindow.DataContext = mainViewModel;

            if (_statusLogWindow.IsVisible)
            {
                _statusLogWindow.Activate();
                return;
            }

            _statusLogWindow.Show();
        }

        #endregion
    }
}