using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilverEx
{
    public static class DelegateHelper
    {
        public static void IsDelegateType(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!typeof(Delegate).IsAssignableFrom(type))
                throw new ArgumentException("type is not an Delegate type.");
        }

        public static MethodInfo GetMethodInfoEx(this Delegate @delegate)
        {
            if (@delegate == null) throw new ArgumentNullException("delegate");
#if !NonDotNetFX
            return @delegate.GetMethodInfo();
#else
            return @delegate.Method;
#endif
        }

        public static TDelegate CreateDelegate<TDelegate>(MethodInfo methodInfo) where TDelegate : class
        {
            return CreateDelegate(typeof(TDelegate), methodInfo).AsTo<TDelegate>();
        }

        public static Delegate CreateDelegate(Type delegateType, MethodInfo methodInfo)
        {
            return CreateDelegate(delegateType, null, methodInfo);
        }

        public static TDelegate CreateDelegate<TDelegate>(object target, MethodInfo methodInfo) where TDelegate : class
        {
            return CreateDelegate(typeof(TDelegate), target, methodInfo).AsTo<TDelegate>();
        }

        public static Delegate CreateDelegate(Type delegateType, object target, MethodInfo methodInfo)
        {
            if (delegateType == null) throw new ArgumentNullException("delegateType");
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");
#if !NonDotNetFX
            return methodInfo.CreateDelegate(delegateType, target);
#else
            return Delegate.CreateDelegate(delegateType, target, methodInfo, true);
#endif
        }
    }
}