using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace SimpleSQLEditor.Services
{
    public class WindowService : IWindowService
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<Type, Window> _openWindows = new();

        #endregion

        #region Constructor

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods & Events

        public void ShowWindow<TWindow>(object viewModel, Action<bool>? onOpenChanged = null)
            where TWindow : Window
        {
            var windowType = typeof(TWindow);

            if (_openWindows.TryGetValue(windowType, out var existingWindow))
            {
                existingWindow.Activate();
                return;
            }

            var window = _serviceProvider.GetRequiredService<TWindow>();

            window.DataContext = viewModel;

            var owner = Application.Current?.MainWindow;
            if (owner is not null && owner != window)
            {
                window.Owner = owner;
            }

            window.Closed += (_, _) =>
            {
                _openWindows.Remove(windowType);
                onOpenChanged?.Invoke(false);
            };

            _openWindows[windowType] = window;

            onOpenChanged?.Invoke(true);

            window.Show();
            window.Activate();
        }

        #endregion
    }
}