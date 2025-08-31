using System.Globalization;
using System.Windows.Data;

namespace FileCloud.Desktop.View.Converters
{
    public class WidthToColumnsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double containerWidth && parameter is string paramStr &&
                double.TryParse(paramStr, NumberStyles.Any, culture, out double itemWidth))
            {
                if (itemWidth <= 0) return 1;

                int columns = (int)Math.Floor(containerWidth / itemWidth);
                return Math.Max(1, columns);
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
