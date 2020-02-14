using System;

namespace Common
{
#if !NETFX_CORE
    using System.Windows.Navigation;
#else
    using Windows.UI.Xaml.Navigation;
#endif

    public class MyNavigationEventArgs
    {
        internal MyNavigationEventArgs(object content, JournalEntry journalEntry, NavigationMode navigationMode)
        {
            this.Journal = journalEntry;
            this.Content = content;
            this.NavigationMode = navigationMode;
        }

        internal JournalEntry Journal { get; private set; }

        public object Content { get; private set; }
        public Type PageType { get { return Journal?.PageType; } }
        public object Parameter { get { return Journal?.Parameter; } }
        public NavigationMode NavigationMode { get; private set; }
    }

    public class MyNavigatingCancelEventArgs
    {
        internal MyNavigatingCancelEventArgs(JournalEntry journalEntry, NavigationMode navigationMode)
        {
            this.Journal = journalEntry;
            this.NavigationMode = navigationMode;
        }

        internal JournalEntry Journal { get; private set; }

        public Type PageType { get { return Journal?.PageType; } }
        public object Parameter { get { return Journal?.Parameter; } }
        public NavigationMode NavigationMode { get; private set; }
        public bool Cancel { get; set; }
    }
}