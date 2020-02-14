using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Common
{
	public static partial class ObjectHelper
	{
        public static bool IsNull(this object obj)
        {
            if (obj == null) return true;
#if WPF
            if (obj == DBNull.Value) return true;
#endif
            return false;
        }

		public static T AsTo<T>(this object obj) where T : class
		{
			return obj as T;
		}

		public static T? AsNullable<T>(this object obj) where T : struct
		{
			return obj as T?;
		}

        public static T CastTo<T>(this object obj, bool coerce = true)
		{
            if (!coerce) return (T)obj;
			T value;
			if (ConverterHelper.TryConvert(obj, out value)) return value;
			throw new InvalidCastException("obj");
		}

        public static dynamic AsDynamic(this object obj)
        {
            return obj;
        }

        public static Exception Try(Action action)
        {
            Exception _ex = null;
            if (action != null)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _ex = ex;
                }
            }
            return _ex;
        }

        public static void TryThrow(this Exception ex)
        {
            if (ex != null) throw ex;
        }

        public static string GetMemberName<T, TMember>(this T obj, Expression<Func<TMember>> memberExpression)
		{
			if (IsNull(obj)) return null;
            if (memberExpression == null) throw new ArgumentNullException("memberExpression");
			var body = memberExpression.Body as MemberExpression;
            if (body == null) throw new ArgumentException("Not A Member", "memberExpression");
            var member = body.Member;
            if (member == null) throw new ArgumentException("Not A Member", "memberExpression");
			return body.Member.Name;
		}

        /// <summary>
        ///   检查两个对象是否相等
        ///   注:两相同类型且相等的值类型对象,装箱后的两对象也是相等的
        ///      两不同类型但相等的值类型对象,装箱后的两对象是不相等的
        /// </summary>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        /// <returns></returns>
        public static bool AreEquals(object object1, object object2)
        {
            if (object1.IsNull() && object2.IsNull()) return true;
            if (object1.IsNull() || object2.IsNull()) return false;
            if (ReferenceEquals(object1, object2)) return true;

            string s1 = object1 as string, s2 = object2 as string;
            if ((s1 != null) && (s2 != null)) return string.Compare(s1, s2, StringComparison.Ordinal) == 0;

            if (object1 == object2) return true;
            if (object1.Equals(object2)) return true;
            if (object2.Equals(object1)) return true;
            return false;
        }

        public static T Tryv<T>(Func<T> func, T defv = default)
        {
            try { return func(); }
            catch { return defv; }
        }
    }
}