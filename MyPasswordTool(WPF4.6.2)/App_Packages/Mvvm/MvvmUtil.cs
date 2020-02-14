using System;
using System.Collections.Generic;

namespace Common
{
#if XAMARIN_FORMS
    using DependencyObject = Xamarin.Forms.BindableObject;
    using DependencyProperty = Xamarin.Forms.BindableProperty;
#elif !NETFX_CORE
    using System.Windows;
#else
    using Windows.UI.Xaml;
#endif

    public class MvvmUtil
    {
        public static readonly IDictionary<Type, Type> Mapper = new Dictionary<Type, Type>();

        public static readonly DependencyProperty IsAutoSetViewModelProperty = dpAttached("IsAutoSetViewModel", typeof(bool), typeof(MvvmUtil), false, on_IsAutoSetViewModelProperty);

        private static void on_IsAutoSetViewModelProperty(DependencyObject v, object oldValue, object newValue)
        {
            if (!(bool)newValue) return;
            SetViewModelToView(v);
        }

        public static bool GetIsAutoSetViewModel(DependencyObject view) => (bool)view.GetValue(IsAutoSetViewModelProperty);
        public static void SetIsAutoSetViewModel(DependencyObject view, bool value) => view.SetValue(IsAutoSetViewModelProperty, value);

        public static void SetViewModelToView(DependencyObject view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            var viewModelType = GetViewModelTypeByViewType(view.GetType());
            if (viewModelType == null) return;

            var viewModel = GetObjectByType(viewModelType);
#if XAMARIN_FORMS
            view.BindingContext = viewModel;
#else
            view.SetValue(FrameworkElement.DataContextProperty, viewModel);
#endif
        }

        public static Func<Type, object> GetObjectByType = (type) => Activator.CreateInstance(type);

        public static void Map(Type viewType, Type viewModelType)
        {
            if (viewType != null && viewModelType != null)
                Mapper.Add(viewType, viewModelType);
        }

        public static Type GetViewModelTypeByViewType(Type viewType)
        {
            return Mapper.TryGetValue(viewType, out var viewModelType) ? viewModelType : null;
        }

        public static Type GetViewTypeByViewModelType(Type viewModelType)
        {
            foreach (var viewType in GetViewTypesByViewModelType(viewModelType))
                return viewType;
            return null;
        }

        public static IEnumerable<Type> GetViewTypesByViewModelType(Type viewModelType)
        {
            foreach (var kv in Mapper)
                if (kv.Value == viewModelType)
                    yield return kv.Key;
        }

        private static DependencyProperty dpAttached(string name, Type returnType, Type ownerType, object defvalue, Action<DependencyObject, object, object> changed = null)
        {
#if XAMARIN_FORMS
            return DependencyProperty.CreateAttached(name, returnType, ownerType, defvalue, propertyChanged: (d, oldValue, newValue) => changed?.Invoke(d, oldValue, newValue));
#else
            return DependencyProperty.RegisterAttached(name, returnType, ownerType, new PropertyMetadata(defvalue, (d, e) => changed?.Invoke(d, e.OldValue, e.NewValue)));
#endif
        }
    }
}