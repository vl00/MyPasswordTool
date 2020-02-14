using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SilverEx
{
    public abstract partial class DynamicBag : INotifyPropertyChanged, IDisposable
    {
        public abstract int Count();
        public abstract IDictionary<string, object> Dictionary();
        public abstract bool Has(string name);
        public abstract object Get(string name);
        public abstract bool TryGet(string name, out object value);
        public abstract void Set(string name, object value);
        public abstract void Release(string name);

        public virtual object this[string name]
        {
            get { return this.Get(name); }
            set { this.Set(name, value); }
        }

        public virtual T Get<T>(string name)
        {
            return (T)Convert.ChangeType(this.Get(name), typeof(T));
        }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual void RaiseChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }

        protected virtual void OnDispose()
        {
            PropertyChanged = null;
        }

        void IDisposable.Dispose()
        {
            OnDispose();
        }
    }
}