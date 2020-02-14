using System;
using System.Reflection;

namespace Common
{
    public delegate bool TryConvertHandler(object value, Type conversionType, IFormatProvider formatProvider, out object convertedValue);

    public static class ConverterHelper
    {
        public static bool TryConvert(object value, Type conversionType, out object convertedValue)
        {
            return TryConvertHandler(value, conversionType, null, out convertedValue);
        }

        public static bool TryConvert<T>(object value, out T convertedValue)
        {
            return TryConvert(value, null, out convertedValue);
        }

        public static bool TryConvert<T>(object value, IFormatProvider formatProvider, out T convertedValue)
        {
            object convertedObject;
            if (TryConvertHandler(value, typeof(T), formatProvider, out convertedObject))
            {
                convertedValue = (T)convertedObject;
                return true;
            }
            convertedValue = default(T);
            return false;
        }

        public static TryConvertHandler TryConvertHandler = (object value, Type conversionType, IFormatProvider formatProvider, out object convertedValue) => defaultTryConvert(value, conversionType, formatProvider, out convertedValue);

        private static bool defaultTryConvert(object value, Type conversionType, IFormatProvider formatProvider, out object convertedValue)
        {
            if (conversionType == null) throw new ArgumentNullException("conversionType");
#if !NonDotNetFX
            var conversionTypeInfo = conversionType.GetTypeInfo();
#else
            var conversionTypeInfo = conversionType;
#endif
            if (conversionType == typeof(object)) 
            {
                convertedValue = value;
                return true;
            }
            if (conversionTypeInfo.IsGenericType && conversionTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value.IsNull())
                {
                    convertedValue = null;
                    return true;
                }
#if !NonDotNetFX
                return defaultTryConvert(value, conversionTypeInfo.GenericTypeArguments[0], formatProvider, out convertedValue);
#else
                return defaultTryConvert(value, conversionType.GetGenericArguments()[0], formatProvider, out convertedValue);
#endif
            }
            if (value == null)
            {
                convertedValue = null;
                return !conversionTypeInfo.IsValueType;
            }
#if WinRT || WPRT
            if (conversionTypeInfo.IsInstanceOfType(value))
#else
            if (conversionType.IsInstanceOfType(value))
#endif
            {
                convertedValue = value;
                return true;
            }
            if (conversionType == typeof(string))
            {
                convertedValue = value.ToString();
                return true;
            }
            if (conversionTypeInfo.IsEnum)
            {
                try
                {
                    var text = value as string;
                    convertedValue = text != null ? Enum.Parse(conversionType, text, false) : Enum.ToObject(conversionType, value);
                    return true;
                }
                catch
                {
                    convertedValue = null;
                    return false;
                }
            }
            if (typeof(Guid).IsAssignableFrom(conversionType))
            {
                var stringValue = value as string;
                if (stringValue != null)
                {
                    convertedValue = new Guid(stringValue);
                    return true;
                }
                var by = value as byte[];
                if (by != null)
                {
                    convertedValue = new Guid(by);
                    return true;
                }
            }
            try
            {
                convertedValue = Convert.ChangeType(value, conversionType, formatProvider);
                return true;
            }
            catch
            {
                convertedValue = null;
                return false;
            }
        }
    }
}