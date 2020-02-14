using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilverEx
{
    public class NotifyProxy : DynamicBag
    {
        public NotifyProxy(object model)
        {
            PropertyInfo[] pis;
            _model = model;
            var type = this.ProxyType = _model.GetType();
            if (!TypeProps.TryGetValue(type.FullName, out pis)) TypeProps[type.FullName] = pis = type.GetRuntimeProperties().ToArray();
            if (!_proxys.TryGetValue(type.FullName, out _proxy)) _proxys[type.FullName] = _proxy = new TypePropertiesProxy(type, pis);
        }

        private object _model;
        private static readonly Dictionary<string, TypePropertiesProxy> _proxys = new Dictionary<string, TypePropertiesProxy>();
        private TypePropertiesProxy _proxy;

        protected static readonly Dictionary<string, PropertyInfo[]> TypeProps = new Dictionary<string, PropertyInfo[]>();

        public Type ProxyType { get; private set; }
        public object Proxy { get { return _model; } }

        public override bool Has(string name)
        {
            return _proxy.HasProperty(name);
        }

        public override object Get(string name)
        {
            return _proxy.GetPropertyValue(_model, name);
        }

        public override bool TryGet(string name, out object value)
        {
            return _proxy.TryGetPropertyValue(_model, name, out value);
        }

        public override void Set(string name, object value)
        {
            var old = _proxy.GetPropertyValue(_model, name);
            if (ObjectHelper.AreEquals(old, value)) return;
            _proxy.SetPropertyValue(_model, name, value);
            this.RaiseChanged(name);
        }

        public override int Count()
        {
            return TypeProps[ProxyType.FullName].Length;
        }

        public override void Release(string name)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Dictionary()
        {
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _model = null;
            _proxy = null;
        }

        private class TypePropertiesProxy
        {
            public TypePropertiesProxy(Type type, PropertyInfo[] pis)
            {
                var _type = type;
                foreach (var pi in pis)
                {
                    if (_proxys.ContainsKey(pi.Name)) continue;
                    _proxys[pi.Name] = new PropertyAccessor(pi);
                }
            }

            private readonly Type _type;
            private readonly Dictionary<string, PropertyAccessor> _proxys = new Dictionary<string, PropertyAccessor>();

            public bool HasProperty(string propertyName)
            {
                return _proxys.ContainsKey(propertyName);
            }

            public object GetPropertyValue(object model, string propertyName)
            {
                PropertyAccessor d;
                if (!_proxys.TryGetValue(propertyName, out d)) throw new ArgumentNullException(propertyName);
                return d.Getter(model);
            }

            public void SetPropertyValue(object model, string propertyName, object value)
            {
                PropertyAccessor d;
                if (!_proxys.TryGetValue(propertyName, out d)) throw new ArgumentNullException(propertyName);
                d.Setter(model, value);
            }

            public bool TryGetPropertyValue(object model, string propertyName, out object value)
            {
                PropertyAccessor d;
                var b = _proxys.TryGetValue(propertyName, out d);
                value = b ? d.Getter(model) : null;
                return b;
            }
        }
    }
}