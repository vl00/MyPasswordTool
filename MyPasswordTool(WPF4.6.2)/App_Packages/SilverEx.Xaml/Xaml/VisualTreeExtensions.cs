using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilverEx
{
#if NETFX_CORE
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;
#else
    using System.Windows;
    using System.Windows.Media;
#endif

    public static class VisualTreeExtensions
    {
#if NETFX_CORE
        public static UIElement GetTopRoot(Window window = null)
        {
            if (window == null) window = Window.Current;

            if (window == null) return null;

            var root = window.Content as FrameworkElement;

            if (root != null)
            {
                var ancestors = root.GetAncestors().ToList();

                if (ancestors.Count > 0)
                {
                    root = (FrameworkElement)ancestors[ancestors.Count - 1];
                }
            }

            return root;
        }

        //public static bool IsInVisualTree(this DependencyObject dob)
        //{
        //    if (AppPlatform.IsDesignTime) return false;
        //
        //    if (Window.Current == null)
        //    {
        //        // This may happen when a picker or CameraCaptureUI etc. is open.
        //        return false;
        //    }
        //
        //    var root = GetTopRoot();
        //
        //    return
        //        root != null && dob.GetAncestors().Contains(root) ||
        //        VisualTreeHelper.GetOpenPopups(Window.Current)
        //            .Any(popup => popup.Child != null && dob.GetAncestors().Contains(popup.Child));
        //}
#endif

        public static bool IsElementLoaded(this FrameworkElement ui)
        {
            if (ui == null) return false;
#if WPF
            return ui.IsLoaded;
#else
            if ((ui.Parent ?? VisualTreeHelper.GetParent(ui)) != null) return true;
            var rootVisual = AppPlatform.RootUI;
            return rootVisual != null && ui == rootVisual;
#endif
        }

        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject ui)
        {
            var parent = ui;
            if (parent == null) yield break;
            while (true)
            {
                if ((parent = VisualTreeHelper.GetParent(parent)) == null) yield break;
                yield return parent;
            }
        }

        public static IEnumerable<T> GetAncestors<T>(this DependencyObject ui) where T : DependencyObject
        {
            return GetAncestors(ui).OfType<T>();
        }

        public static DependencyObject FindSelfOrAncestor(this DependencyObject ui, Type ancestorType = null, int? ancestorLevel = null)
        {
            var i = ancestorLevel == null || ancestorLevel.Value < 0 ? -1 : ancestorLevel.Value;
            if (ui == null || (i < 0 && ancestorType == null)) return ui;
            if (i == 0) return (ancestorType == null || ancestorType.IsInstanceOfType(ui)) ? ui : null;
            var ps = ancestorType == null ? ui.GetAncestors() : ui.GetAncestors().Where(p => ancestorType.IsInstanceOfType(p));
            return ps.ElementAt(i < 0 ? 0 : i - 1);
        }

        public static T FindSelfOrAncestor<T>(this DependencyObject ui, int? ancestorLevel = null) where T : DependencyObject
        {
            return (T)FindSelfOrAncestor(ui, typeof(T), ancestorLevel);
        }

        /// <summary>
        /// 广度查找
        /// </summary>
        /// <param name="ui"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetDescendants(this DependencyObject ui)
        {
            if (ui != null)
            {
                var queue = new Queue<DependencyObject>();
                var p = ui;
                var i = -1;
                do
                {
                    if (i != -1) p = queue.Dequeue();
                    i = 0;
                    for (var j = VisualTreeHelper.GetChildrenCount(p); i < j; i++)
                    {
                        var c = VisualTreeHelper.GetChild(p, i);
                        queue.Enqueue(c);
                        yield return c;
                    }
                } while (queue.Count > 0);
            }
        }

        /// <summary>
        /// 深度查找
        /// </summary>
        /// <param name="ui"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetDescendantsDepth(this DependencyObject ui)
        {
            if (ui == null) yield break;
            for (int i = 0, j = VisualTreeHelper.GetChildrenCount(ui); i < j; i++)
            {
                var cc = VisualTreeHelper.GetChild(ui, i);
                yield return cc;
                foreach (var c in GetDescendantsDepth(cc))
                    yield return c;
            }
            yield break;
        }

        public static IEnumerable<T> GetDescendants<T>(this DependencyObject ui) where T : DependencyObject
        {
            return GetDescendants(ui).OfType<T>();
        }

        public static IEnumerable<T> GetDescendantsDepth<T>(this DependencyObject ui) where T : DependencyObject
        {
            return GetDescendantsDepth(ui).OfType<T>();
        }

        public static object FindStaticResource(this FrameworkElement ui, string resourceKey)
        {
            if (ui == null) throw new ArgumentNullException(nameof(ui));
            if (resourceKey == null) return null;
#if !WPF
            if (keyInResourceDictionary(ui.Resources, resourceKey)) return ui.Resources[resourceKey];
            var parent = ui.GetAncestors<FrameworkElement>().FirstOrDefault(f => keyInResourceDictionary(f.Resources, resourceKey));
            if (parent != null) return parent.Resources[resourceKey];
            if (keyInResourceDictionary(Application.Current.Resources, resourceKey)) return Application.Current.Resources[resourceKey];
            return null;
#else
            return ui.TryFindResource(resourceKey);
#endif
        }

        private static bool keyInResourceDictionary(ResourceDictionary resourceDictionary, string resourceKey)
        {
#if NETFX_CORE
            return resourceDictionary.ContainsKey(resourceKey);
#else
            return resourceDictionary.Contains(resourceKey);
#endif
        }
    }
}