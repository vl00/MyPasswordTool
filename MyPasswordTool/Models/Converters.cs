using Common;
using MyPasswordTool.Models;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyPasswordTool
{
    //public abstract class MyValueConverter : IValueConverter
    //{
    //    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    /// <summary>
    /// 用于调试
    /// </summary>
    public class DebugValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class BoolNullValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            return value;
        }
    }

    public class ByteArrayToBitmapImageValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.AsTo<byte[]>().LoadToBitmapImage();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ByteArrayToImageBackgroundValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.AsTo<byte[]>() != null) return new SolidColorBrush(Colors.White);
            else return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.CastTo<bool>() ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolNullToVisibilityValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.CastTo<bool?>() == true ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PaTagsToTagStringValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Join(",", (value.AsTo<IEnumerable<PaTag>>() ?? new PaTag[0]).Select(t => t.Name));
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
