using System;
using System.Collections;
using System.Collections.Generic;

namespace Common
{
#if !NETFX_CORE
    using System.Windows;
#else
    using Windows.UI.Xaml;
#endif

    public interface IJournalsSerializer
    {
        string Serialize(int currentIndex, JournalEntry[] journals);
        void Deserialize(string serializedJournals, out int currentIndex, out JournalEntry[] journals);
    }

    public class JournalEntry : DependencyObject
    {
        public JournalEntry(Type pageType, object parameter)
        {
            PageType = pageType;
            Parameter = parameter;
            ParameterType = parameter?.GetType() ?? typeof(object);
        }

        public Type PageType { get; private set; }
        public object Parameter { get; private set; }
        public Type ParameterType { get; private set; }
    }

    public class JournalsCollection : IEnumerable<JournalEntry>
    {
        internal JournalsCollection()
        {
            j_BackStack = new JournalEntryList(this);
            j_ForwardStack = new JournalEntryList(this);
        }

        private readonly JournalEntryList j_BackStack;
        private readonly JournalEntryList j_ForwardStack;
        private int _MaxBackStackDepth;

        public IList<JournalEntry> BackStack { get { return j_BackStack; } }
        public IList<JournalEntry> ForwardStack { get { return j_ForwardStack; } }

        public JournalEntry Current { get; private set; }

        public JournalEntry Previous
        {
            get 
            {
                var c = j_BackStack.Count;
                return c > 0 ? j_BackStack[c - 1] : null;
            }
        }

        public JournalEntry Next
        {
            get
            {
                var c = j_ForwardStack.Count;
                return c > 0 ? j_ForwardStack[0] : null;
            }
        }

        public int MaxBackStackDepth 
        {
            get { return _MaxBackStackDepth; }
            internal set 
            {
                _MaxBackStackDepth = value;
                j_BackStack.Resize();
            }
        }

        public int BackStackDepth
        {
            get { return j_BackStack.Count; }
        }

        public int CurrentIndex
        {
            get 
            {
                if (Current == null) return -1;
                return j_BackStack.Count;
            }
        }

        public IEnumerator<JournalEntry> GetEnumerator()
        {
            foreach (var j in BackStack)
                yield return j;

            yield return Current;

            foreach (var j in ForwardStack)
                yield return j;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal void GoBack()
        {
            var c = j_BackStack.Count;
            if (c > 0)
            {
                if (Current != null) j_ForwardStack.Insert(0, Current);
                Current = j_BackStack[c - 1];
                j_BackStack.RemoveAt(c - 1);
            }
        }

        internal void GoForward()
        {
            var c = j_ForwardStack.Count;
            if (c > 0)
            {
                if (Current != null) j_BackStack.Add(Current);
                Current = j_ForwardStack[0];
                j_ForwardStack.RemoveAt(0);
            }
        }

        internal void GoNew(JournalEntry journalEntry)
        {
            if (journalEntry == null) return;
            if (Current != null) j_BackStack.Add(Current);
            Current = journalEntry;
            j_ForwardStack.Clear();
        }

        internal void Refresh(JournalEntry journalEntry)
        {
            if (journalEntry == null) return;
            Current = journalEntry;
        }

        public void Clear()
        {
            Current = null;
            j_BackStack.Clear();
            j_ForwardStack.Clear();
        }

        private class JournalEntryList : IList<JournalEntry>
        {
            public JournalEntryList(JournalsCollection journals)
            {
                this.journals = journals;
            }

            private readonly JournalsCollection journals;
            private readonly List<JournalEntry> ls = new List<JournalEntry>();

            public int IndexOf(JournalEntry item)
            {
                return ls.IndexOf(item);
            }

            public void Add(JournalEntry item)
            {
                if (item == null) throw new ArgumentNullException("JournalEntry");
                if (journals_is_empty()) throw new InvalidOperationException("journals is empty");
                ls.Add(item);
                Resize();
            }

            public void Insert(int index, JournalEntry item)
            {
                if (item == null) throw new ArgumentNullException("JournalEntry");
                if (journals_is_empty()) throw new InvalidOperationException("journals is empty");
                ls.Insert(index, item);
                Resize();
            }

            public void RemoveAt(int index)
            {
                ls.RemoveAt(index);
            }

            public bool Remove(JournalEntry item)
            {
                return ls.Remove(item);
            }

            public JournalEntry this[int index]
            {
                get { return ls[index]; }
                set 
                {
                    if (index < 0 || index >= ls.Count) throw new ArgumentOutOfRangeException("index");
                    ls[index] = value;
                }
            }

            public void Clear()
            {
                ls.Clear();
            }

            public bool Contains(JournalEntry item)
            {
                return ls.Contains(item);
            }

            public void CopyTo(JournalEntry[] array, int arrayIndex)
            {
                ls.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return ls.Count; }
            }

            public IEnumerator<JournalEntry> GetEnumerator()
            {
                return ls.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ls.GetEnumerator();
            }

            bool ICollection<JournalEntry>.IsReadOnly
            {
                get { return false; }
            }

            private bool journals_is_empty()
            {
                return journals.Current == null;
            }

            internal void Resize()
            {
                if (ReferenceEquals(this, journals.BackStack))
                {
                    while (ls.Count > journals.MaxBackStackDepth)
                        this.RemoveAt(0);
                }
            }
        }
    }
}