using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SilverEx
{
    public static partial class IoC
    {
        public static object Resolve(Type type, string name = null) => ResolveFactory(type, name);
        public static T Resolve<T>(string name = null) => (T)ResolveFactory(typeof(T), name);
        public static IEnumerable ResolveAll(Type type) => ResolveAllFactory(type);
        public static IEnumerable<T> ResolveAll<T>() => ResolveAllFactory(typeof(T)).Cast<T>();
        public static void Inject<T>(T instance) => InjectFactory(instance);
    }

    public static partial class IoC
    {
        private static Func<Type, string, object> ResolveFactory = delegate { throw ex_not_invoked(nameof(SetResolve)); };
        private static Func<Type, IEnumerable<object>> ResolveAllFactory = delegate { throw ex_not_invoked(nameof(SetResolveAll)); };
        private static Action<object> InjectFactory = delegate { throw ex_not_invoked(nameof(SetInject)); };

        public static void SetResolve(Func<Type, string, object> factory) => ResolveFactory = factory;
        public static void SetResolveAll(Func<Type, IEnumerable<object>> factory) => ResolveAllFactory = factory;
        public static void SetInject(Action<object> factory) => InjectFactory = factory;

        private static Exception ex_not_invoked(string methodName) => new InvalidOperationException($"IoC.{methodName} is not invoked ever.");
    }
}