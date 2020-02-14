using System;
using System.ComponentModel;

namespace SilverEx
{
#if NETFX_CORE
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;
#else
    using System.Windows;
    using System.Windows.Data;
#endif

    public static class BindingHelper
    {
        public static object GetBindingValue(object source, string path)
        {
            return GetBindingValue(new Binding { Source = source, Path = new PropertyPath(path), Mode = BindingMode.OneTime });
        }

        public static object GetBindingValue(Binding binding)
        {
            var f = new BindableDependencyObject();
            f.SetBinding(BindableDependencyObject.ValueProperty, binding);
            return f.Value;
        }

        public static void ClearBinding(this DependencyObject target, DependencyProperty dp, bool revert = false)
        {
            object v = null;
            if (!revert) v = target.GetValue(dp);
#if WPF
            BindingOperations.ClearBinding(target, dp);
#else
            target.ClearValue(dp);
#endif
            if (!revert) target.SetValue(dp, v);
        }

        /// <summary>
        /// 1.binding的数据源为可绑定对象，例如:
        ///     1).实现通知如:INotifyPropertyChanged,WinRT可以是IObservableMap<string, object>
        ///     2).WinRT下没实现任何通知借口但在VisualTree中的DependencyObject,非WinR下的任意DependencyObject
        /// 2.当DependencyObject为数据源时,注册的DependencyProperty要使用PropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback);
        /// 3.target可以为任意DependencyObject,在WinRT下TwoWay绑定时可以改变普通数据源属性,但此普通数据源的更改无法通知到target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dp"></param>
        /// <param name="binding"></param>
        public static void SetBinding(this DependencyObject target, DependencyProperty dp, BindingBase binding)
        {
            BindingOperations.SetBinding(target, dp, binding);
        }

        public static BindingExpression GetBindingExpression(this DependencyObject target, DependencyProperty dp)
        {
#if WPF
            return BindingOperations.GetBindingExpression(target, dp);
#else
            return target.ReadLocalValue(dp) as BindingExpression;
#endif
        }

        /// <summary>
        /// 只能监听使用DependencyProperty.Register注册的依赖属性
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dpName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IDisposable ListenDependencyPropertyChanged(this DependencyObject d, string dpName, Action<IDisposable, DependencyPropertyChangedEventArgs> callback)
        {
            if (d == null) throw new ArgumentNullException(nameof(d));

            var x = new BindableDependencyObject();
            SetBinding(x, BindableDependencyObject.ValueProperty, new Binding { Source = d, Path = new PropertyPath(dpName), Mode = BindingMode.OneWay });

            var eh = new PropertyChangedCallback((_, e) => callback(x, e));
            x.Disposing += delegate { x.ValueChanged -= eh; };
            x.ValueChanged += eh;

            return x;
        }

#if WPF || WINDOWS_UWP
        /// <summary>
        /// uwp/wpf专用
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dp"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IDisposable ListenDependencyPropertyChanged(this DependencyObject d, DependencyProperty dp, Action<IDisposable, DependencyPropertyChangedEventArgs> callback)
        {
            if (d == null) throw new ArgumentNullException(nameof(d));
            if (dp == null) throw new ArgumentNullException(nameof(dp));

            var x = new BindableDependencyObject { Value = d.GetValue(dp) };
            var eh = new PropertyChangedCallback((_, e) => callback(x, e));
#if WINDOWS_UWP
            var r = d.RegisterPropertyChangedCallback(dp, delegate { x.Value = d.GetValue(dp); });

            x.Disposing += delegate 
            {
                x.ValueChanged -= eh;
                d.UnregisterPropertyChangedCallback(dp, r);
            };
#elif WPF
            var dpd = DependencyPropertyDescriptor.FromProperty(dp, d.GetType());
            var h = new EventHandler(delegate { x.Value = d.GetValue(dp); });
            dpd.AddValueChanged(d, h);

            x.Disposing += delegate
            {
                x.ValueChanged -= eh;
                dpd.RemoveValueChanged(d, h);
            };
#endif
            x.ValueChanged += eh;
            return x;
        }
#endif

        private class BindableDependencyObject : DependencyObject, IDisposable
        {
            public event PropertyChangedCallback ValueChanged;
            public event EventHandler Disposing;

            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("Value", typeof(object), typeof(BindableDependencyObject),
                    new PropertyMetadata(default(object), on_ValuePropertyChanged));

            private static void on_ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                (d as BindableDependencyObject)?.ValueChanged?.Invoke(d, e);
            }

            public object Value
            {
                get { return GetValue(ValueProperty); }
                set { SetValue(ValueProperty, value); }
            }

            public void Dispose()
            {
                Disposing?.Invoke(this, EventArgs.Empty);
                Disposing = null;
            }
        }
    }
}