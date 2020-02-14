using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SilverEx
{
    public class PropertyAccessor
    {
        public PropertyAccessor(PropertyInfo pi)
        {
            this.PropertyInfo = pi;
            if (pi.CanRead) Getter = BuildGetter(pi);
            if (pi.CanWrite) Setter = BuildSetter(pi);
        }

        public PropertyInfo PropertyInfo { get; private set; }
        public Func<object, object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }

        public static Func<object, object> BuildGetter(PropertyInfo pi)
        {
            var mi_get = pi.GetGetMethod();
            var method = new DynamicMethod("Get", typeof(object), new[] { typeof(object) });
            var il = method.GetILGenerator();
            il.DeclareLocal(typeof(object));
            if (!mi_get.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                castType4MethodSource(il, pi.DeclaringType);
            }
            callMethod(il, mi_get);
            if (mi_get.ReturnType.IsValueType) il.Emit(OpCodes.Box, mi_get.ReturnType);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            method.DefineParameter(1, ParameterAttributes.In, "value");
            return method.CreateDelegate(typeof(Func<object, object>)) as Func<object, object>;
        }

        public static Action<object, object> BuildSetter(PropertyInfo pi)
        {
            var mi_set = pi.GetSetMethod();
            var paramType = pi.PropertyType;
            var method = new DynamicMethod("Set", null, new[] { typeof(object), typeof(object) });
            var il = method.GetILGenerator();
            il.DeclareLocal(paramType);
            if (!mi_set.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                castType4MethodSource(il, pi.DeclaringType);
            }
            il.Emit(OpCodes.Ldarg_1);
            if (paramType.IsValueType)
            {
                il.Emit(OpCodes.Unbox, paramType);
                il.Emit(OpCodes.Ldobj, paramType);
            }
            else il.Emit(OpCodes.Castclass, paramType);
            callMethod(il, mi_set);
            il.Emit(OpCodes.Ret);
            method.DefineParameter(1, ParameterAttributes.In, "obj");
            method.DefineParameter(2, ParameterAttributes.In, "value");
            return method.CreateDelegate(typeof(Action<object, object>)) as Action<object, object>;
        }

        private static void castType4MethodSource(ILGenerator il, Type type)
        {
            var l = il.DeclareLocal(type);
            il.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
            il.Emit(OpCodes.Stloc, l);
            if (type.IsValueType) il.Emit(OpCodes.Ldloca_S, l);
            else il.Emit(OpCodes.Ldloc, l);
        }

        private static void callMethod(ILGenerator il, MethodInfo method, Type[] optionalParameterTypes = null)
        {
            if (method.IsStatic || method.DeclaringType.IsValueType) il.EmitCall(OpCodes.Call, method, optionalParameterTypes);
            else il.EmitCall(OpCodes.Callvirt, method, optionalParameterTypes);
        }
    }
}