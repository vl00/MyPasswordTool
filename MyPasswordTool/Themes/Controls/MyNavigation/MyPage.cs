using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
#if !NETFX_CORE
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Navigation;
#else
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Markup;
    using Windows.UI.Xaml.Navigation;
#endif

#if NETFX_CORE
    [ContentProperty(Name = "Content")]
    public class MyPage : UserControl
#else
    [ContentProperty("Content")]
    public class MyPage : UserControl
#endif
    {
        public MyPage()
        {
            this.DefaultStyleKey = typeof(MyPage);
        }

#if NETFX_CORE
        private Page _pageWapper;
#else
        private ContentPresenter _pageWapper;
#endif
        private bool _is_pageCreated;
        private MyFrame m_frame;

        public MyFrame Frame 
        {
            get { return m_frame; }
            internal set
            {
                if (m_frame != value)
                {
                    var old_frame = m_frame;
                    m_frame = value;
                    OnFrameChanged(old_frame, m_frame);
                }
            }
        }

#if NETFX_CORE

        public static readonly DependencyProperty TopAppBarProperty =
            DependencyProperty.Register("TopAppBar", typeof(AppBar), typeof(MyPage), new PropertyMetadata(default(AppBar)));

        public AppBar TopAppBar
        {
            get => (AppBar)GetValue(TopAppBarProperty);
            set => SetValue(TopAppBarProperty, value);
        }

        public static readonly DependencyProperty BottomAppBarProperty =
            DependencyProperty.Register("BottomAppBar", typeof(AppBar), typeof(MyPage), new PropertyMetadata(default(AppBar)));

        public AppBar BottomAppBar
        {
            get => (AppBar)GetValue(BottomAppBarProperty); 
            set => SetValue(BottomAppBarProperty, value); 
        }

#endif

        public static readonly DependencyProperty NavigationCacheModeProperty =
            DependencyProperty.Register("NavigationCacheMode", typeof(NavigationCacheMode), typeof(MyPage),
                new PropertyMetadata(NavigationCacheMode.Enabled, OnNavigationCacheModePropertyChanged));

        private static void OnNavigationCacheModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = (MyPage)d;
            var oldv = (NavigationCacheMode)e.OldValue;
            var newv = (NavigationCacheMode)e.NewValue;
            if (oldv == newv) return;
            page.OnNavigationCacheModeChanged(oldv, newv);
        }

        public NavigationCacheMode NavigationCacheMode
        {
            get => (NavigationCacheMode)GetValue(NavigationCacheModeProperty);
            set => SetValue(NavigationCacheModeProperty, value);
        }

        protected virtual Task OnNavigatingFromAsync(MyNavigatingCancelEventArgs e) { return null; }
        protected virtual void OnNavigatedFrom(MyNavigationEventArgs e) { }
        protected virtual void OnNavigatedTo(MyNavigationEventArgs e) { }

#if NETFX_CORE
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _pageWapper = GetTemplateChild("page_root") as Page;
        }
#else
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _pageWapper = GetTemplateChild("page_root") as ContentPresenter;
        }
#endif

        internal void FireOnPageCreated()
        {
            if (_is_pageCreated) return;
            _is_pageCreated = true;
            this.ApplyTemplate();
        }

        internal async Task FireOnNavigatingFromAsync(MyNavigatingCancelEventArgs e)
        {
            var task = OnNavigatingFromAsync(e);
            if (task != null) await task;
        }

        internal void FireOnNavigatedFrom(MyNavigationEventArgs e) => OnNavigatedFrom(e);

        internal void FireOnNavigatedTo(MyNavigationEventArgs e) => OnNavigatedTo(e);

        private void OnFrameChanged(MyFrame oldFrame, MyFrame newFrame)
        {
            oldFrame?.PageFactory.ReleasePage(this);
        }

        private void OnNavigationCacheModeChanged(NavigationCacheMode oldValue, NavigationCacheMode newValue)
        {
            Frame?.PageFactory.AddOrReleasePage(this);
        }
    }
}