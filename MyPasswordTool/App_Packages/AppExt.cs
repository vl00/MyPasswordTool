using Common;
using SilverEx;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MyPasswordTool
{
    public static partial class AppExt
    {
        public static T BagToModel<T>(this NotifyBag bag)
        {
            var type = typeof(T);
            var m = Activator.CreateInstance(type);
            var pis = type.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public);
            foreach (var pi in pis)
            {
                pi.SetValue(m, bag.Get(pi.Name), null);
            }
            return (T)m;
        }

        public static T ModelToBag<T>(this object m) where T : NotifyBag, new()
        {
            var type = m.GetType();
            var bag = new T();
            var pis = type.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public);
            foreach (var pi in pis)
            {
                bag.Set(pi.Name, pi.GetValue(m, null));
            }
            return bag;
        }

        public static BitmapImage LoadToBitmapImage(this byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.None;//.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.DecodePixelWidth = 80;
                image.DecodePixelHeight = 80;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static IDisposable ListenPropertyChanged(this INotifyPropertyChanged n, string name, Action<PropertyChangedEventArgs> action)
        {
            return ListenPropertyChanged(n, e => e.PropertyName == name, action);
        }

        public static IDisposable ListenPropertyChanged(this INotifyPropertyChanged n, Func<PropertyChangedEventArgs, bool> func, Action<PropertyChangedEventArgs> action)
        {
            void _on_(object _, PropertyChangedEventArgs e)
            {
                if ((func == null || func(e)) && action != null)
                    action(e);
            }

            n.PropertyChanged += _on_;
            return new Disposable(_off_);

            void _off_() => n.PropertyChanged -= _on_;
        }

        public static IDisposable ListenPropertyChanged<T>(this T n, Func<T, PropertyChangedEventArgs, bool> func, Action<T, PropertyChangedEventArgs> action)
            where T : INotifyPropertyChanged
        {
            void _on_(object _, PropertyChangedEventArgs e)
            {
                if (action != null && _ is T o && (func == null || func(o, e)))
                    action(o, e);
            }

            n.PropertyChanged += _on_;
            return new Disposable(_off_);

            void _off_() => n.PropertyChanged -= _on_;
        }

    }
}
