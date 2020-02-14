using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilverEx
{
    public static partial class ReflectionHelper
    {
#if WinRT || WPRT
        public static bool IsAssignableFrom(this Type type, Type c)
        {
            if (c == null) return false;
            if (type == c) return true;
            if (c.GetTypeInfo().IsSubclassOf(c)) return true;
            if (type.GetTypeInfo().IsInterface) return c.ImplementInterface(type);
            if (!type.IsGenericParameter) return false;
            var genericParameterConstraints = type.GetTypeInfo().GetGenericParameterConstraints();
            return genericParameterConstraints.All(t => IsAssignableFrom(t, c));
        }

        private static bool ImplementInterface(this Type type, Type ifaceType)
        {
            while (type != null)
            {
                var interfaces = type.GetTypeInfo().ImplementedInterfaces.ToArray();
                if (interfaces != null)
                {
                    if (interfaces.Any(t => t == ifaceType || (t != null && t.ImplementInterface(ifaceType))))
                        return true;
                }
                type = type.GetTypeInfo().BaseType;
            }
            return false;
        }

        public static bool IsInstanceOfType(this Type type, object o)
        {
            return o != null && IsAssignableFrom(type, o.GetType());
        }

        public static bool IsInstanceOfType(this TypeInfo typeInfo, object o)
        {
            return IsInstanceOfType(typeInfo.AsType(), o);
        }
#endif

#if NonDotNetFX
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }

        public static Type AsType(this Type type)
        {
            return type;
        }
#endif

#if !NonDotNetFX
        public static TypeInfo GetObjTypeInfo(this object obj)
#else
        public static Type GetObjTypeInfo(this object obj)
#endif
        {
            return obj == null ? null : obj.GetType().GetTypeInfo();
        }
    }
}