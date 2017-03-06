using System;
using System.Globalization;
using System.Windows;

namespace Wpf.Backup.Converters
{
    public class BoolToVisibilityConverter : BaseConverter<BoolToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool val = (bool)value;

                string param = parameter as string;
                if (!string.IsNullOrEmpty(param) && param.Contains("i"))
                    val = !val;
                if (val)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
    }
}
