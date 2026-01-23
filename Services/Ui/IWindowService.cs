using System.Windows;

namespace SimpleSQLEditor.Services.Ui
{
    public interface IWindowService
    {
        void ShowWindow<TWindow>(object viewModel, Action<bool>? onOpenChanged = null)
            where TWindow : Window;
    }
}