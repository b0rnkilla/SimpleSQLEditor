using SimpleSQLEditor.ViewModels;
using SimpleSQLEditor.Views;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleSQLEditor.Services
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

        public event EventHandler<bool>? IsStatusLogOpenChanged;

        public void ShowStatusLog()
        {
            if (_statusLogWindow == null)
            {
                _statusLogWindow = _serviceProvider.GetRequiredService<StatusLogWindow>();
                _statusLogWindow.Closed += (_, _) =>
                {
                    _statusLogWindow = null;
                    IsStatusLogOpenChanged?.Invoke(this, false);
                };

                IsStatusLogOpenChanged?.Invoke(this, true);
            }

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