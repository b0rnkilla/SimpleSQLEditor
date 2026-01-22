using System.Globalization;
using System.Windows.Data;

namespace SimpleSQLEditor.Infrastructure
{
    public sealed class EnumEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || parameter is null)
                return false;

            var valueText = value.ToString();
            var parameterText = parameter.ToString();

            return string.Equals(valueText, parameterText, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is null)
                return Binding.DoNothing;

            if (value is bool isChecked && !isChecked)
                return Binding.DoNothing;

            var parameterText = parameter.ToString();
            if (string.IsNullOrWhiteSpace(parameterText))
                return Binding.DoNothing;

            return Enum.Parse(targetType, parameterText, ignoreCase: true);
        }
    }
}