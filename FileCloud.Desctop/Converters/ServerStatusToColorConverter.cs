using FileCloud.Desktop.Models;
using FileCloud.Desktop.View.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace FileCloud.Desktop.View.Converters
{
    public class ServerStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServerStatus status)
            {
                return status switch
                {
                    ServerStatus.Online => Brushes.Green,
                    ServerStatus.Offline => Brushes.Red,
                    ServerStatus.Connecting => Brushes.Yellow,
                    ServerStatus.Error => Brushes.Orange,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
