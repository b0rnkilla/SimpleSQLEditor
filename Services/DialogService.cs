using System.Windows;

namespace SimpleSQLEditor.Services
{
    public class DialogService : IDialogService
    {
        public bool Confirm(string title, string message)
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }
    }
}