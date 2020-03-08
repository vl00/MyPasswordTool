using System;
using System.Collections;
using System.Collections.Generic;

namespace Common
{
    public partial class SimpleInstanceContainer : IDisposable
    {
        public class Config : IDisposable
        {
            private readonly List<InstanceFactory> insFactories = new List<InstanceFactory>();

            private IDisposable Set(InstanceLifetime lifetime, Type type, string key, Func<SimpleInstanceContainer, object> valueFactory)
            {
                var insf = new InstanceFactory(insFactories, lifetime, type, key, valueFactory);
                insFactories.Add(insf);
                return insf;
            }
            private IDisposable Set<T>(InstanceLifetime lifetime, string key, Func<SimpleInstanceContainer, T> valueFactory) => Set(lifetime, typeof(T), key, c => valueFactory(c));

            public IDisposable AddTransient(Type type, string key, Func<SimpleInstanceContainer, object> valueFactory) => Set(InstanceLifetime.Transient, type, key, valueFactory);
            public IDisposable AddTransient<T>(string key, Func<SimpleInstanceContainer, T> valueFactory) => Set(InstanceLifetime.Transient, key, valueFactory);
            public IDisposable AddTransient<T>(Func<SimpleInstanceContainer, T> valueFactory) => Set(InstanceLifetime.Transient, null, valueFactory);

            public IDisposable AddSingleton(Type type, string key, Func<SimpleInstanceContainer, object> valueFactory) => Set(InstanceLifetime.Singleton, type, key, valueFactory);
            public IDisposable AddSingleton<T>(string key, Func<SimpleInstanceContainer, T> valueFactory) => Set(InstanceLifetime.Singleton, key, valueFactory);
            public IDisposable AddSingleton<T>(Func<SimpleInstanceContainer, T> valueFactory) => Set(InstanceLifetime.Singleton, null, valueFactory);
            public IDisposable AddSingleton<T>(string key, T value) => Set(InstanceLifetime.Singleton, key, _ => value);
            public IDisposable AddSingleton<T>(T value) => Set(InstanceLifetime.Singleton, null, _ => value);

            public IDisposable AddScoped(Type type, string key, Func<SimpleInstanceContainer, object> valueFactory) => Set(InstanceLifetime.Scoped, type, key, valueFactory);
            public IDisposable AddScoped<T>(string key, Func<SimpleInstanceContainer, T> valueFactory) => Set(InstanceLifetime.Scoped, key, valueFactory);
            public IDisposable AddScoped<T>(Func<SimpleInstanceContainer, T> valueFactory) => Set(InstanceLifetime.Scoped, null, valueFactory);

            public void RemoveAll(Type type, string key = null)
            {
                insFactories.RemoveAll(insf => insf.Type == type && (key == null || insf.Key == key));
                //var clean = new List<InstanceFactory>();
                //foreach (var insf in insFactories)
                //{
                //    if (insf.Type == type && (key == null || insf.Key == key))
                //        clean.Add(insf);
                //}
                //foreach (var insf in clean)
                //{
                //    insf.Dispose();
                //}
            }

            public SimpleInstanceContainer CreateContainer() => new SimpleInstanceContainer(this, insFactories);

            public void Dispose() => insFactories.Clear();
        }

        private readonly string id = DateTime.Now.Ticks + "_" + Guid.NewGuid().ToString();
        private readonly SimpleInstanceContainer parent;
        private readonly IList<InstanceFactory> insFactories;
        private readonly Config config;

        public event EventHandler Disposing;

        private SimpleInstanceContainer(Config config, IList<InstanceFactory> insFactories, SimpleInstanceContainer parent = null)
        {
            this.parent = parent;
            this.config = config;
            this.insFactories = insFactories;
        }

        public Config GetConfig() => this.config;

        public SimpleInstanceContainer CreateScopeContainer() => new SimpleInstanceContainer(config, insFactories, this);

        public T Get<T>(string key = null) => (T)Get(typeof(T), key);

        public object Get(Type type, string key = null)
        {
            TryGet(type, key, out var value);
            return value;
        }

        public bool TryGet(Type type, string key, out object value)
        {
            InstanceFactory _insf = null;
            foreach (var insf in insFactories)
            {
                if (insf.Type == type && (key == null || insf.Key == key))
                {
                    _insf = insf;
                    break;
                }
            }
            value = _insf?.GetInstance(this);
            return _insf != null;
        }

        public IEnumerable GetAll(Type type, string key = null) => GetAll((t, k) => t == type && (key == null || k == key));
        public IEnumerable GetAll(string key) => GetAll((_, k) => k == key);

        public IEnumerable GetAll(Func<Type, string, bool> func)
        {
            foreach (var insf in insFactories)
            {
                if (func?.Invoke(insf.Type, insf.Key) != false)
                    yield return insf.GetInstance(this);
            }
        }

        public IEnumerable<T> GetAll<T>(string key = null)
        {
            foreach (var insf in insFactories)
            {
                if (insf.Type == typeof(T) && (key == null || insf.Key == key))
                    yield return (T)insf.GetInstance(this);
            }
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);
            Disposing = null;
        }

        public override string ToString() => $"_$$SimpleInstanceContainer:{id}";

        private SimpleInstanceContainer getRoot()
        {
            var p = this;
            while (p.parent != null)
                p = p.parent;
            return p;
        }
    }

    partial class SimpleInstanceContainer
    {
        private enum InstanceLifetime { Transient, Singleton, Scoped }

        private class InstanceFactory : IDisposable
        {
            public InstanceFactory(IList<InstanceFactory> factories, InstanceLifetime lifetime, Type type, string key, Func<SimpleInstanceContainer, object> valueFactory)
            {
                this.factories = factories;
                this.Lifetime = lifetime;
                this.valueFactory = valueFactory;
                this.Type = type;
                this.Key = key;

                single_or_scoped_instances = lifetime != InstanceLifetime.Transient ? new Dictionary<string, object>() : null;
            }

            private IList<InstanceFactory> factories;
            private Func<SimpleInstanceContainer, object> valueFactory;

            private IDictionary<string, object> single_or_scoped_instances;
            private readonly object _locker = new object();

            public Type Type { get; }
            public string Key { get; }
            public InstanceLifetime Lifetime { get; }

            public object GetInstance(SimpleInstanceContainer container)
            {
                if (container == null) throw new ArgumentNullException(nameof(container));
                if (factories == null) throw new ObjectDisposedException(nameof(InstanceFactory));

                switch (Lifetime)
                {
                    case InstanceLifetime.Transient:
                        return get_instance_by_Transient(container);
                    case InstanceLifetime.Singleton:
                        return get_instance_by_Singleton(container);
                    case InstanceLifetime.Scoped:
                        return get_instance_by_Scoped(container);
                    default:
                        throw new NotImplementedException();
                }
            }

            private object get_instance_by_Transient(SimpleInstanceContainer container) => valueFactory?.Invoke(container);

            private object get_instance_by_Singleton(SimpleInstanceContainer container)
            {
                object o = null;
                var root = container.getRoot();
                var id = root.id;
                lock (_locker)
                {
                    if (!single_or_scoped_instances.TryGetValue(id, out o))
                    {
                        single_or_scoped_instances.Add(id, o = valueFactory(root));
                        add_on_container_Disposing(root, new WeakReference(this));
                    }
                }
                return o;
            }

            private object get_instance_by_Scoped(SimpleInstanceContainer container)
            {
                object o = null;
                var id = container.id;
                lock (_locker)
                {
                    if (single_or_scoped_instances.TryGetValue(id, out o))
                        return o;
                    single_or_scoped_instances.Add(id, o = valueFactory(container));
                }
                add_on_container_Disposing(container, new WeakReference(this));
                return o;
            }

            public void Dispose()
            {
                if (factories == null) return;
                lock (_locker)
                {
                    factories.Remove(this);
                    factories = null;
                    valueFactory = null;
                    single_or_scoped_instances = null;
                }
            }

            public override string ToString() => $"[{Lifetime}]{(Key == null ? "" : $"[Key={Key}]")}{{Type={Type}}}";

            private static void add_on_container_Disposing(SimpleInstanceContainer container, WeakReference factory_ref)
            {
                var id = container.id;
                container.Disposing += delegate
                {
                    var @this = factory_ref.Target as InstanceFactory;
                    if (@this?.single_or_scoped_instances == null) return;
                    lock (@this._locker)
                        @this.single_or_scoped_instances?.Remove(id);
                };
            }
        }
    }
}