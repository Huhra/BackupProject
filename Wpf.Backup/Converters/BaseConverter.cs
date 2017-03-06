using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Backup.Converters
{
    public class BaseConverter<T> : IValueConverter
        where T : new()
    {
        private static readonly Lazy<T> Lazy = new Lazy<T>(() => new T());
        public static T Instance => Lazy.Value;

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
