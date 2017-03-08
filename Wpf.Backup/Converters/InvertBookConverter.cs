using System;
using System.Globalization;

namespace Wpf.Backup.Converters
{
    public class InvertBookConverter : BaseConverter<InvertBookConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return !((bool)value);
            return value;
        }
    }
}
