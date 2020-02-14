using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static partial class CollectionHelper
    {
        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return collection == null || collection.Count <= 0;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        public static void Foreach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (action == null || enumerable == null) return;
            foreach (var item in enumerable)
                action(item);
        }

        public static void Foreach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            if (action == null || enumerable == null) return;
            var i = 0;
            foreach (var item in enumerable)
            {
                action(item, i);
                i++;
            }
        }

        public static void PredicateEach<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            if (predicate == null || enumerable == null) return;
            foreach (var item in enumerable)
                if (!predicate(item)) break;
        }

        public static void PredicateEach<T>(this IEnumerable<T> enumerable, Func<T, int, bool> predicate)
        {
            if (predicate == null || enumerable == null) return;
            var i = 0;
            foreach (var item in enumerable)
            {
                if (!predicate(item, i)) break;
                i++;
            }
        }

        public static bool In<T>(this T item, IEnumerable<T> enumerable, Predicate<T> predicate = null)
        {
            if (enumerable == null) return false;
            predicate = predicate ?? (i => ObjectHelper.AreEquals(item, i));
            return enumerable.Any(i => predicate(i));
        }

        public static bool In(this object item, params object[] collection)
        {
            return In(item, collection.AsEnumerable());
        }

        public static TV GetValueEx<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defValue = default(TV))
        {
            TV v;
            if (dict.TryGetValue(key, out v)) return v;
            return defValue;
        }
    }
}