using System;
using System.Collections.Generic;
using static Common.ObjectHelper;

namespace SilverEx
{
    public partial class NotifyBag : DynamicBag
    {
        public NotifyBag() : this(null) { }

        public NotifyBag(IDictionary<string, object> dict)
        {
            _dict = dict != null ? new Dictionary<string, object>(dict) : new Dictionary<string, object>();
        }

        private IDictionary<string, object> _dict;

        public override bool Has(string name)
        {
            return _dict.ContainsKey(name);
        }

        public override object Get(string name)
        {
            object v;
            _dict.TryGetValue(name, out v);
            return v;
        }

        public override bool TryGet(string name, out object value)
        {
            return _dict.TryGetValue(name, out value);
        }

        public override void Set(string name, object value)
        {
            var old = this.Get(name);
            if (AreEquals(old, value)) return;
            _dict[name] = value;
            this.RaiseChanged(name);
        }

        public override int Count()
        {
            return _dict.Count;
        }

        public override IDictionary<string, object> Dictionary()
        {
            return _dict;
        }

        public override void Release(string name)
        {
            _dict.Remove(name);
            this.RaiseChanged(name);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _dict = null;
        }
    }
}