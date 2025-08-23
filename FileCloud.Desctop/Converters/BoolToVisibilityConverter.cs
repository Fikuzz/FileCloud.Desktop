using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FileCloud.Desktop.View.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool b) return Visibility.Collapsed;

            bool invert = parameter != null && parameter.ToString() == "Invert";
            if (invert) b = !b;

            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility v) return false;

            bool result = v == Visibility.Visible;
            bool invert = parameter != null && parameter.ToString() == "Invert";
            return invert ? !result : result;
        }
    }
}
