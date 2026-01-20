using System.Windows;

namespace SimpleSQLEditor.Services
{
    public interface IWindowService
    {
        void ShowWindow<TWindow>(object viewModel, Action<bool>? onOpenChanged = null)
            where TWindow : Window;
    }
}