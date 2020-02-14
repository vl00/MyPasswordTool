using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilverEx
{
	public static partial class ReflectionHelper
	{
		public static T ChangeType<T>(this object source)
		{
			if (source == null) return default(T);
			if (source.GetType() == typeof(T)) return (T)source;
			return (T)ChangeType(source, typeof(T));
		}

		public static object ChangeType(this object source, Type newType)
		{
#if !SILVERLIGHT
			return Convert.ChangeType(source, newType);
#else
			return Convert.ChangeType(source, newType, null);
#endif
		}

		public static object GetDefaultValue(this Type type)
		{
			if (!type.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(type) != null) return null;
			return Activator.CreateInstance(type);
		}

#if WinRT || WPRT
        public static IEnumerable<FieldInfo> GetFieldsEx(this Type type, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            var source = new List<FieldInfo>();
            var baseType = type;
            while ((baseType == type) || ((baseType != null) && (baseType != typeof(object))))
            {
                source.AddRange(from member in baseType.GetTypeInfo().DeclaredFields
                                where (baseType == type || !member.IsPrivate)
                                   && (allowStatic || member.IsStatic == allowStatic)
                                   && (allowPublic || member.IsPublic == allowPublic)
                                   && (allowNoPublic || member.IsPublic != allowNoPublic)
                                select member);
                baseType = baseType.GetTypeInfo().BaseType;
            }
            return source.Distinct();
        }

        public static IEnumerable<MethodInfo> GetMethodsEx(this Type type, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            var source = new List<MethodInfo>();
            var baseType = type;
            while ((baseType == type) || ((baseType != null) && (baseType != typeof(object))))
            {
                source.AddRange(from member in baseType.GetTypeInfo().DeclaredMethods
                                where (baseType == type || !member.IsPrivate)
                                   && (allowStatic || member.IsStatic == allowStatic)
                                   && (allowPublic || member.IsPublic == allowPublic)
                                   && (allowNoPublic || member.IsPublic != allowNoPublic)
                                select member);
                baseType = baseType.GetTypeInfo().BaseType;
            }
            return source.Distinct();
        }

        public static IEnumerable<PropertyInfo> GetPropertiesEx(this Type type, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            var source = new List<PropertyInfo>();
            var baseType = type;
            while ((baseType == type) || ((baseType != null) && (baseType != typeof(object))))
            {
                source.AddRange(from member in baseType.GetTypeInfo().DeclaredProperties
                                select member);
                baseType = baseType.GetTypeInfo().BaseType;
            }
            return source.Distinct();
        }

        public static IEnumerable<EventInfo> GetEventsEx(this Type type, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            var source = new List<EventInfo>();
            var baseType = type;
            while ((baseType == type) || ((baseType != null) && (baseType != typeof(object))))
            {
                source.AddRange(from member in baseType.GetTypeInfo().DeclaredEvents
                                select member);
                baseType = baseType.GetTypeInfo().BaseType;
            }
            return source.Distinct();
        }
#else
        public static IEnumerable<FieldInfo> GetFieldsEx(this Type type, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            var flags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            if (!allowPublic) flags = flags.ClearFlag(BindingFlags.Public);
            if (!allowNoPublic) flags = flags.ClearFlag(BindingFlags.NonPublic);
            if (!allowStatic) flags = flags.ClearFlag(BindingFlags.Static);
            return type.GetFields(flags);
        }

        public static IEnumerable<MethodInfo> GetMethodsEx(this Type type, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            var flags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            if (!allowPublic) flags = flags.ClearFlag(BindingFlags.Public);
            if (!allowNoPublic) flags = flags.ClearFlag(BindingFlags.NonPublic);
            if (!allowStatic) flags = flags.ClearFlag(BindingFlags.Static);
            return type.GetMethods(flags);
        }

        public static IEnumerable<PropertyInfo> GetPropertiesEx(this Type type, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            var flags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            if (!allowPublic) flags = flags.ClearFlag(BindingFlags.Public);
            if (!allowNoPublic) flags = flags.ClearFlag(BindingFlags.NonPublic);
            if (!allowStatic) flags = flags.ClearFlag(BindingFlags.Static);
            return type.GetProperties(flags);
        }

        public static IEnumerable<EventInfo> GetEventsEx(this Type type, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            var flags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            if (!allowPublic) flags = flags.ClearFlag(BindingFlags.Public);
            if (!allowNoPublic) flags = flags.ClearFlag(BindingFlags.NonPublic);
            if (!allowStatic) flags = flags.ClearFlag(BindingFlags.Static);
            return type.GetEvents(flags);
        }
#endif
        public static FieldInfo GetField(this Type type, string name, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException("name");
            return GetFieldsEx(type, allowPublic: allowPublic, allowNoPublic: allowNoPublic, allowStatic: allowStatic).FirstOrDefault(x => x.Name == name);
        }

        public static MethodInfo GetMethod(this Type type, string name, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException("name");
            return GetMethodsEx(type, allowPublic: allowPublic, allowNoPublic: allowNoPublic, allowStatic: allowStatic).FirstOrDefault(x => x.Name == name);
        }

        public static PropertyInfo GetProperty(this Type type, string name, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException("name");
            return GetPropertiesEx(type, allowPublic: allowPublic, allowNoPublic: allowNoPublic, allowStatic: allowStatic).FirstOrDefault(x => x.Name == name);
        }

        public static EventInfo GetEvents(this Type type, string name, bool allowPublic = true, bool allowNoPublic = false, bool allowStatic = true)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException("name");
            return GetEventsEx(type, allowPublic: allowPublic, allowNoPublic: allowNoPublic, allowStatic: allowStatic).FirstOrDefault(x => x.Name == name);
        }
    }
}