using System;
using System.Reflection;

namespace SilverEx
{
#if NETFX_CORE
    using Windows.UI.Xaml.Data;
#else
    using System.Globalization;
    using System.Windows.Data;
#endif

    public class TypeValueConverter : IValueConverter
    {
#if !NETFX_CORE
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
#else
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
#endif
        protected virtual object Convert(object value, Type targetType, object parameter)
        {
            if (value == null) return Type.Missing;
            if (value is Type) return value;
#if !NonDotNetFX
            if (value is TypeInfo) return ((TypeInfo)value).AsType();
#endif
            if (value is string) return Type.GetType(value.ToString(), true);
            throw new NotImplementedException();
        }
    }
}