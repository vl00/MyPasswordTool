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
    using Windows.UI.Xaml.Media.Animation;
    using Windows.UI.Xaml.Navigation;
#endif

#if NETFX_CORE
    [ContentProperty(Name = "Content")]
#else
    [ContentProperty("Content")]
#endif
    public partial class MyFrame : Control
    {
        public MyFrame()
        {
            this.DefaultStyleKey = typeof(MyFrame);
            m_PageFactory = new MyPageFactory() { Frame = this };
            m_Journals = new JournalsCollection() { MaxBackStackDepth = this.MaxBackJournalsCount };
        }

        private ContentPresenter frame_root;
        private bool isbusying;
        private readonly JournalsCollection m_Journals;
        private readonly MyPageFactory m_PageFactory;
        private IJournalsSerializer m_JournalsSerializer;

        public bool IsBusying { get { return isbusying; } }
        public bool IsSuspending { get; private set; }
        public bool IsResuming { get; private set; }
        public bool IsNavigating { get; private set; }

        internal MyPageFactory PageFactory => m_PageFactory;

        public JournalsCollection Journals => m_Journals;

        public bool CanGoBack => !isbusying && Journals.BackStack.Count > 0;
        public bool CanGoForward => !isbusying && Journals.ForwardStack.Count > 0;

        public IJournalsSerializer JournalsSerializer
        {
            get
            {
                if (m_JournalsSerializer == null) m_JournalsSerializer = CreateJournalsSerializerFunc();
                return m_JournalsSerializer;
            }
        }

        public event EventHandler<MyNavigatingCancelEventArgs> Navigating;
        public event EventHandler<MyNavigationEventArgs> Navigated;

        public static Func<IJournalsSerializer> CreateJournalsSerializerFunc = () => null;
        public static Func<Type, MyPage> CreatePageFunc = (pageType) => Activator.CreateInstance(pageType) as MyPage; 

        public static readonly DependencyProperty MaxBackJournalsCountProperty =
           DependencyProperty.Register("MaxBackJournalsCount", typeof(int), typeof(MyFrame),
                new PropertyMetadata(10, OnMaxBackJournalsCountPropertyChanged));

        private static void OnMaxBackJournalsCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var i = (int)e.NewValue;
            if (i < 0) throw new ArgumentException("MaxBackJournalsCount");
            (d as MyFrame).Journals.MaxBackStackDepth = i;
        }

        public int MaxBackJournalsCount
        {
            get { return (int)GetValue(MaxBackJournalsCountProperty); }
            set { SetValue(MaxBackJournalsCountProperty, value); }
        }

        public static readonly DependencyProperty CacheSizeProperty =
            DependencyProperty.Register("CacheSize", typeof(int), typeof(MyFrame),
                new PropertyMetadata(10, OnCacheSizePropertyChanged));

        private static void OnCacheSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldv = (int)e.OldValue;
            var newv = (int)e.NewValue;
            if (oldv == newv) return;
            (d as MyFrame).PageFactory.CacheSize = newv;
        }

        public int CacheSize
        {
            get { return (int)GetValue(CacheSizeProperty); }
            set { SetValue(CacheSizeProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(MyFrame), new PropertyMetadata(null));

        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(MyFrame), new PropertyMetadata(null));

        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

#if NETFX_CORE

        public static readonly DependencyProperty ContentTransitionsProperty =
            DependencyProperty.Register("ContentTransitions", typeof(TransitionCollection), typeof(MyFrame), new PropertyMetadata(null));

        public TransitionCollection ContentTransitions
        {
            get { return (TransitionCollection)GetValue(ContentTransitionsProperty); }
            set { SetValue(ContentTransitionsProperty, value); }
        }

#endif

#if NETFX_CORE
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            frame_root = GetTemplateChild("frame_root") as ContentPresenter;
        }

        public async Task<bool> GoBackAsync()
        {
            if (!CanGoBack) return false;
            return await run_Navigation_Async(Journals.Current, Journals.Previous, NavigationMode.Back);
        }

        public async Task<bool> GoForwardAsync()
        {
            if (!CanGoForward) return false;
            return await run_Navigation_Async(Journals.Current, Journals.Next, NavigationMode.Forward);
        }

        public Task<bool> NavigateAsync(Type pageType)
        {
            return NavigateAsync(pageType, null);
        }

        public Task<bool> NavigateAsync(Type pageType, object parameter)
        {
            return run_Navigation_Async(Journals.Current, new JournalEntry(pageType, parameter), NavigationMode.New);
        }

        public string GetNavigationState(bool clearup = false)
        {
            if (Journals.Current == null) return null;
            if (isbusying) throw new InvalidOperationException("frame is busying");
            try
            {
                isbusying = true;
                this.IsSuspending = true;

                var e = new MyNavigationEventArgs(this.Content, Journals.Current, NavigationMode.Forward);
                fire_page_OnNavigatedFrom(this.Content as MyPage, e);

                var str = JournalsSerializer?.Serialize(Journals.CurrentIndex, Journals.ToArray());

                if (clearup)
                {
                    Journals.Clear();
                    this.Content = null;
                }

                return str;
            }
            finally 
            {
                this.IsSuspending = false;
                isbusying = false;
            }
        }

        public void SetNavigationState(string navigationState)
        {
            if (isbusying) throw new InvalidOperationException("frame is busying");
            try
            {
                isbusying = true;
                this.IsHitTestVisible = false;
                this.IsResuming = true;

                int currentIndex = -1;
                JournalEntry[] journals = null;
                JournalsSerializer?.Deserialize(navigationState, out currentIndex, out journals);

                Journals.Clear();
                if (journals == null || journals.Length <= 0 || currentIndex < 0)
                {
                    this.Content = null;
                    return;
                }
                Journals.Refresh(journals[currentIndex]);
                for (var i = 0; i < journals.Length; i++)
                {
                    if (i == currentIndex) continue;
                    var j = journals[i];
                    if (j == null) throw new InvalidOperationException();
                    if (i < currentIndex) Journals.BackStack.Add(j);
                    else Journals.ForwardStack.Add(j);
                }

                var page = m_PageFactory.GetOrCreatePage(Journals.Current);
                var e = new MyNavigationEventArgs(page, Journals.Current, NavigationMode.Back);
                fire_page_OnNavigatedTo(page, e);
            }
            finally
            {
                this.IsResuming = false;
                this.IsHitTestVisible = true;
                isbusying = false;
            }
        }

        private async Task<bool> run_Navigation_Async(JournalEntry source, JournalEntry target, NavigationMode mode)
        {
            if (isbusying) return false;
            isbusying = true;
            this.IsHitTestVisible = false;
            try
            {
                var sourcePage = this.Content as MyPage;

                if (!(await call_OnNavigatingAsync(sourcePage, source, target, mode))) return false;

                switch (mode)
                {
                    case NavigationMode.New:
                        Journals.GoNew(target);
                        break;
                    case NavigationMode.Back:
                        Journals.GoBack();
                        break;
                    case NavigationMode.Forward:
                        Journals.GoForward();
                        break;
                    case NavigationMode.Refresh:
                        Journals.Refresh(target);
                        break;
                    default:
                        throw new NotSupportedException("NavigationMode");
                }

                m_PageFactory.AddOrReleasePage(sourcePage);

                var targetPage = m_PageFactory.GetOrCreatePage(target);

                return call_OnNavigated(sourcePage, source, targetPage, target, mode);
            }
            finally
            {
                this.IsHitTestVisible = true;
                isbusying = false;
            }
        }

        private async Task<bool> call_OnNavigatingAsync(MyPage sourcePage, JournalEntry source, JournalEntry target, NavigationMode mode)
        {
            var e = new MyNavigatingCancelEventArgs(target, mode);
            try
            {
                e.Cancel = false;
                IsNavigating = true;
                fire_frame_Navigating(this, e);
                await fire_page_OnNavigatingFromAsync(sourcePage, e);
            }
            finally { IsNavigating = false; }
            if (e.Cancel)
            {
                //fire_frame_NavigationStopped
                return false;
            }
            return true;
        }

        private bool call_OnNavigated(MyPage sourcePage, JournalEntry source, MyPage targetPage, JournalEntry target, NavigationMode mode)
        {
            try
            {
                var e = new MyNavigationEventArgs(targetPage, target, mode);
                fire_frame_Navigated(this, e);
                fire_page_OnNavigatedFrom(sourcePage, e);
                fire_page_OnNavigatedTo(targetPage, e);
            }
            catch (Exception ex)
            {
                //fire_frame_NavigationFailed
                //return false;
                throw ex;
            }
            return true;
        }

        private static void fire_frame_Navigating(MyFrame frame, MyNavigatingCancelEventArgs e)
        {
            frame?.Navigating?.Invoke(frame, e);
        }

        private static async Task fire_page_OnNavigatingFromAsync(MyPage page, MyNavigatingCancelEventArgs e)
        {
            if (!e.Cancel && page != null)
                await page.FireOnNavigatingFromAsync(e);
        }

        private static void fire_frame_Navigated(MyFrame frame, MyNavigationEventArgs e)
        {
            frame?.Navigated?.Invoke(frame, e);
        }

        private static void fire_page_OnNavigatedFrom(MyPage page, MyNavigationEventArgs e)
        {
            page?.FireOnNavigatedFrom(e);
        }

        private static void fire_page_OnNavigatedTo(MyPage page, MyNavigationEventArgs e)
        {
            page?.FireOnNavigatedTo(e);
        }
    }

    public partial class MyFrame
    {
        internal class MyPageFactory 
        {
            private readonly List<MyPage> _requiredCache = new List<MyPage>();
            private readonly List<MyPage> _limitedSizeCache = new List<MyPage>();
            private int m_cacheSize;

            public MyFrame Frame { get; set; }

            public int CacheSize
            {
                get { return m_cacheSize; }
                set 
                {
                    m_cacheSize = value;
                    ResizeCache();
                }
            }

            public MyPage GetOrCreatePage(JournalEntry journal)
            {
                MyPage page = null;
                if (journal == null) return null;

                var len = Math.Max(_limitedSizeCache.Count, _requiredCache.Count);
                for (var i = len - 1; i >= 0; i--)
                {
                    page = GetByIndex(_limitedSizeCache, i);
                    if (page != null && page.GetType() == journal.PageType)
                    {
                        _limitedSizeCache.RemoveAt(i);
                        break;
                    }

                    page = GetByIndex(_requiredCache, i);
                    if (page != null && page.GetType() == journal.PageType)
                    {
                        _requiredCache.RemoveAt(i);
                        break;
                    }

                    page = null;
                }

                if (page == null) page = CreatePageFunc(journal.PageType);
                if (page == null) throw new NullReferenceException("page is null or page isn't a instance of MyPage");

                //page.Frame = null;
                if (Frame.Content != page) Frame.Content = page;
                page.FireOnPageCreated();
                page.Frame = Frame;

                if (page.NavigationCacheMode != NavigationCacheMode.Disabled)
                {
                    if (page.NavigationCacheMode == NavigationCacheMode.Enabled)
                        _limitedSizeCache.Add(page);
                    else if (page.NavigationCacheMode == NavigationCacheMode.Required)
                        _requiredCache.Add(page);
                    ResizeCache();
                }

                return page;
            }

            public void AddOrReleasePage(MyPage page)
            {
                if (page != null && m_cacheSize > 0)
                {
                    ReleasePage(page);
                    if (page.NavigationCacheMode != NavigationCacheMode.Disabled)
                    {
                        if (page.NavigationCacheMode == NavigationCacheMode.Enabled)
                            _limitedSizeCache.Add(page);
                        else if (page.NavigationCacheMode == NavigationCacheMode.Required)
                            _requiredCache.Add(page);
                    }
                }
                ResizeCache();
            }

            public void ReleasePage(MyPage page)
            {
                RemoveAll(_limitedSizeCache, page);
                RemoveAll(_requiredCache, page);
            }

            private void ResizeCache()
            {
                if (m_cacheSize <= 0)
                {
                    _limitedSizeCache.Clear();
                    _requiredCache.Clear();
                }
                else
                {
                    while (_limitedSizeCache.Count > m_cacheSize)
                        _limitedSizeCache.RemoveAt(0);
                }
            }

            private static T GetByIndex<T>(List<T> list, int index)
            {
                if (index < 0 || list == null || list.Count <= 0 || list.Count <= index) return default(T);
                return list[index];
            }

            private static void RemoveAll<T>(List<T> list, T item) where T : class
            {
                list.RemoveAll(t => ReferenceEquals(item, t) || t == item);
            }
        }
    }
}