using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if WinRT
    using Windows.Foundation;
    using Windows.UI.Xaml.Data;
#endif

namespace SilverEx.DataVirtualization
{
    /// <summary>
    /// Derived VirtualizatingCollection, performing loading asychronously.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    public partial class AsyncVirtualizingCollection<T> : VirtualizingCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageTimeout">The page timeout.</param>
        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize = 100, int pageTimeout = 10000)
            : base(itemsProvider, pageSize, pageTimeout) 
        {
            this.SyncContext = SynchronizationContext.Current;
        }

        public AsyncVirtualizingCollection(Func<int, int, Task<IEnumerable<T>>> fetchRange, Func<Task<int>> fetchCount, int pageSize = 100, int pageTimeout = 10000)
            : this(new FuncItemsProvider<T>(fetchRange, fetchCount), pageSize, pageTimeout) { }

        #region SynchronizationContext
        public SynchronizationContext SyncContext { get; private set; }
        #endregion

        #region INotifyCollectionChanged

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        public virtual void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = CollectionChanged;
            if (h != null) h(this, e);
        }

        /// <summary>
        /// Fires the collection reset event.
        /// </summary>
        private void FireCollectionReset()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            RaiseCollectionChanged(e);
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var h = PropertyChanged;
            if (h != null) h(this, e);
        }

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaisePropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(e);
        }

        #endregion

        #region IsLoading IsInitializing

        private bool _isLoading;

        /// <summary>
        /// Gets or sets a value indicating whether the collection is loading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this collection is loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if (value != _isLoading)
                {
                    _isLoading = value;
                    RaisePropertyChanged("IsLoading");
                }
            }
        }

        private bool _isInitializing;

        public bool IsInitializing
        {
            get
            {
                return _isInitializing;
            }
            set
            {
                if (value != _isInitializing)
                {
                    _isInitializing = value;
                    RaisePropertyChanged("IsInitializing");
                }
            }
        }
        #endregion

        public virtual void Refesh()
        {
            this.Count = -1;
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Item[]");
            FireCollectionReset();
        }

        #region Load overrides

        internal void ForceLoadCount()
        {
            LoadCount();
        }

        /// <summary>
        /// Asynchronously loads the count of items.
        /// </summary>
        protected override void LoadCount()
        {
            if (Count == 0)
                IsInitializing = true;

            LoadCountAsync();
        }

        /// <summary>
        /// Performed on background thread.
        /// </summary>
        /// <param name="args">None required.</param>
        private async void LoadCountAsync()
        {
            int count = await FetchCount();

            //update count and reset collection only in case of new count differs from what we have now
            if (this.Count != count)
                this.TakeNewCount(count);
            IsInitializing = false;
        }

        private void TakeNewCount(int newCount)
        {
            if (newCount != this.Count)
            {
                this.Count = newCount;
                this.EmptyCache();
                FireCollectionReset();
            }
        }

        /// <summary>
        /// Asynchronously loads the page.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void LoadPage(int pageIndex, int pageLength)
        {
            IsLoading = true;
            //ThreadPool.QueueUserWorkItem(LoadPageWork, new int[] { pageIndex, pageLength });
            LoadPageAsync(pageIndex, pageLength);
        }

        /// <summary>
        /// Performed on background thread.
        /// </summary>
        /// <param name="args">Index of the page to load.</param>
        private async void LoadPageAsync(int pageIndex, int pageLength)
        {
            int overallCount = 0;
            IList<T> dataItems = await FetchPage(pageIndex, pageLength);//, out overallCount);
            overallCount = await ItemsProvider.FetchCount(); //dataItems.Count;

            this.TakeNewCount(overallCount);
            PopulatePage(pageIndex, dataItems);
            IsLoading = false;
        }

        #endregion
    }

#if WinRT
    public partial class AsyncVirtualizingCollection<T> : ISupportIncrementalLoading
    {
        private bool _hasMoreItems = true;
        /// <summary>
        /// Gets/sets whether the ListViewBase (e.g. GridView/ListView) should continue loading data from
        /// source
        /// </summary>
        public bool HasMoreItems
        {
            get
            {
                CheckMoreItemsAsync();
                return _hasMoreItems;
            }
            set
            {
                _hasMoreItems = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HasMoreItems"));
            }
        }

        private async void CheckMoreItemsAsync()
        {
            int updatedCount = await ItemsProvider.FetchCount();
            bool result = updatedCount > this.Count;
            HasMoreItems = result;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return new IncrementalLoader<T>(this, count);
        }
    }

    /// <summary>
    /// This class is responsible for retrieving data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IncrementalLoader<T> : IAsyncOperation<LoadMoreItemsResult>
        where T: class
    {
        private AsyncStatus _asyncStatus = AsyncStatus.Started;
        private LoadMoreItemsResult _results;

        public IncrementalLoader(AsyncVirtualizingCollection<T> collection, uint count)
        {
            PullDataIncrementalData(collection, count);
        }

        public async void PullDataIncrementalData(AsyncVirtualizingCollection<T> collection, uint count)
        {
            try
            {
                collection.IsLoading = true;
                int nextPageIndex = collection.LastPageIndex + 1;

                collection.ForceLoadCount();

                //IEnumerable<T> newItems = await incrementalLoadingCollection.PullDataAsync(count);

                //if (newItems != null)
                //{
                //    if (newItems.Count() > 0)
                //    {
                //        foreach (T item in newItems)
                //        {
                //            incrementalLoadingCollection.Add(item);
                //        }
                //    }
                //    else
                //    {
                //        incrementalLoadingCollection.HasMoreItems = false;
                //    }
                //}
                //else
                //{
                //    throw new InvalidDataException();
                //}

                // On success, increment page
                _asyncStatus = AsyncStatus.Completed;
                //incrementalLoadingCollection.CurrentPage++;
            }
            catch
            {
                _results.Count = 0;
                _asyncStatus = AsyncStatus.Error;
                collection.HasMoreItems = false;
            }

            collection.IsLoading = false;
            if (Completed != null)
            {
                Completed(this, _asyncStatus);
            }
        }

        public AsyncOperationCompletedHandler<LoadMoreItemsResult> Completed { get; set; }

        public LoadMoreItemsResult GetResults()
        {
            return _results;
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
        }

        public Exception ErrorCode
        {
            get { throw new NotImplementedException(); }
        }

        public uint Id
        {
            get { throw new NotImplementedException(); }
        }

        public AsyncStatus Status
        {
            get { return _asyncStatus; }
        }
    }
#endif
}
