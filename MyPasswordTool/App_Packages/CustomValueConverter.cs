using System;

namespace Common
{
#if !NETFX_CORE
    using System.Globalization;
    using System.Windows.Data;
#else
    using Windows.UI.Xaml.Data;
#endif

    public class CustomValueConverter : IValueConverter
    {
#if NETFX_CORE
        public Func<object, Type, object, string, object> ConvertFunc { get; set; }
        public Func<object, Type, object, string, object> ConvertBackFunc { get; set; }
#else
        public Func<object, Type, object, CultureInfo, object> ConvertFunc { get; set; }
        public Func<object, Type, object, CultureInfo, object> ConvertBackFunc { get; set; }
#endif

#if NETFX_CORE
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            var func = ConvertFunc;
            if (func != null)
                return func(value, targetType, parameter, language);
            else
                throw new NotImplementedException();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var func = ConvertBackFunc;
            if (func != null)
                return func(value, targetType, parameter, language);
            else
                throw new NotImplementedException();
        }
#else
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var func = ConvertFunc;
            if (func != null)
                return func(value, targetType, parameter, culture);
            else
                throw new NotImplementedException();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var func = ConvertBackFunc;
            if (func != null)
                return func(value, targetType, parameter, culture);
            else
                throw new NotImplementedException();
        }
#endif
    }
}