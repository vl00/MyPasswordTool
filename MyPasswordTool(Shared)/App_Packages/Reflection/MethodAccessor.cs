using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SilverEx
{
    public class MethodAccessor
    {
        public MethodAccessor(MethodInfo method)
        {
            MethodInfo = method;
            Invoker = BuildInvoker(method);
        }

        public Func<object, object[], object> Invoker { get; private set; }
        public MethodInfo MethodInfo { get; private set; }

        public static Func<object, object[], object> BuildInvoker(MethodInfo method)
        {
            var dm = new DynamicMethod("Invoke", typeof(object), new[] { typeof(object), typeof(object) });
            var il = dm.GetILGenerator();
            var ps = method.GetParameters();
            var ptypes = new Type[ps.Length];
            var locals = new LocalBuilder[ps.Length];
            for (var i = 0; i < ps.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef) ptypes[i] = ps[i].ParameterType.GetElementType();
                else ptypes[i] = ps[i].ParameterType;
                locals[i] = il.DeclareLocal(ptypes[i], true);
                if (ps[i].IsOut) continue;
                il.Emit(OpCodes.Ldarg_1);
                EmitFastIndex(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                castType(il, ptypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }

            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                var l = il.DeclareLocal(method.DeclaringType);
                castType(il, method.DeclaringType);
                il.Emit(OpCodes.Stloc, l);
                if (method.DeclaringType.IsValueType) il.Emit(OpCodes.Ldloca_S, l);
                else il.Emit(OpCodes.Ldloc, l);
            }
            for (var i = 0; i < locals.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef) il.Emit(OpCodes.Ldloca_S, locals[i]);
                else il.Emit(OpCodes.Ldloc, locals[i]);
            }

            callMethod(il, method);
            if (method.ReturnType == typeof(void)) il.Emit(OpCodes.Ldnull);
            else tryBox(il, method.ReturnType);

            for (var i = 0; i < locals.Length; i++)
            {
                if (ps[i].IsOut || ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastIndex(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    tryBox(il, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
            dm.DefineParameter(1, ParameterAttributes.In, "obj");
            dm.DefineParameter(2, ParameterAttributes.In, "ps");
            return dm.CreateDelegate(typeof(Func<object, object[], object>)) as Func<object, object[], object>;
        }

        private static void tryBox(ILGenerator il, Type type)
        {
            if (type.IsValueType) il.Emit(OpCodes.Box, type);
        }

        private static void castType(ILGenerator il, Type type)
        {
            il.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
        }

        private static void callMethod(ILGenerator il, MethodInfo method, Type[] optionalParameterTypes = null)
        {
            if (method.IsStatic || method.DeclaringType.IsValueType) il.EmitCall(OpCodes.Call, method, optionalParameterTypes);
            else il.EmitCall(OpCodes.Callvirt, method, optionalParameterTypes);
        }

        private static void EmitFastIndex(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }
            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }
}