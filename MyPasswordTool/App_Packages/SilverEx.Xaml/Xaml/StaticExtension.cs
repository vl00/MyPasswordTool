using Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Markup;
using Expression = System.Linq.Expressions.Expression;

namespace SilverEx.Xaml
{
    public sealed class StaticExtension : MarkupExtension
    {
        public string Member { get; set; }
        public Type MemberType { get; set; }

        private const BindingFlags _flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        private static readonly Dictionary<(Type, string), Func<object>> _functions = new Dictionary<(Type, string), Func<object>>();

        public static Func<object> CreateFunc(Type type, string memberName)
        {
            Expression body;
            if (type.IsEnum)
            {
                body = Expression.Constant(Enum.Parse(type, memberName, false), typeof(object));
            }
            else
            {
                var member = type.GetField(memberName, _flags) ??
                             (MemberInfo)type.GetProperty(memberName, _flags);
                if (member == null)
                {
                    throw new MissingMemberException(string.Concat(type.FullName, ".", memberName));
                }
                Expression memberExpression = Expression.MakeMemberAccess(null, member);
                body = !memberExpression.Type.IsValueType
                           ? memberExpression
                           : Expression.Convert(memberExpression, typeof(object));
            }
            return Expression.Lambda<Func<object>>(body).Compile();
        }

        public static Func<object> GetFunc(Type type, string memberName)
        {
            Func<object> function;
            lock (_functions)
            {
                var key = (type, memberName);
                if (!_functions.TryGetValue(key, out function))
                {
                    _functions.Add(key, function = CreateFunc(type, memberName));
                }
            }
            return function;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var type = MemberType;
            string[] tokens = null;
            if (string.IsNullOrEmpty(this.Member) || string.IsNullOrWhiteSpace(this.Member)
                || (type == null && (tokens = this.Member.Split('.')).Length != 2))
            {
                throw new InvalidOperationException("Member property must be set to a non-empty value on Static markup extension.");
            }
            var typeResolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
            var value = GetFunc(type ?? typeResolver.Resolve(tokens[0]), type != null ? Member : tokens[1])();
            if (value != null)
            {
                var targetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                MemberInfo member;
                object convertedValue;
                if ((member = targetProvider.TargetProperty as MemberInfo) != null &&
                    ConverterHelper.TryConvert(value, member.MemberType == MemberTypes.Property
                                                          ? ((PropertyInfo) member).PropertyType
                                                          : ((MethodInfo) member).ReturnType, out convertedValue))
                {
                    return convertedValue;
                }
            }
            return value;
        }
    }
}