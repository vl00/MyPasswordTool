/* --------------------------------------------------------------------------------------------------------------------------------------------------------------
 * -- @rg<rgtype>_<rgAttrName>[<imsp>]   
 *      rgtype 
 *          eg: ff, A/attr
 *      rgAttrName
 *          for complie
 *      imsp
 *          (__[<order:\d>]<name>[<index:\d>])...
 * --------------------------------------------------------------------------------------------------------------------------------------------------------------
 * -- @rgff_store_sub__<name> 
 *          it is a method for rg like 'IEnumerable<UnSubscribe> @rgff_store_sub__fewfg45(Store<StrCls> store){...}'
 *      name
 *          same as define method name
 * --------------------------------------------------------------------------------------------------------------------------------------------------------------
 * -- @rgAttr_store_sub__[<order:\d>]<methodname>          
 *          as a attr for attach to a method, like 
 *          ```
 *              static int? 
 *                  @rgAttr_store_sub__0f0(StrCls state) => state.ACls?.I;
 *              static int?
 *                  @rgAttr_store_sub__1f0(StrCls state) => state.ACls?.I;
 *              void f0(int? nv, int? ov) { }
 *          ```
 *      order
 *          for the rgattrs order by
 *      methodname
 *          the method for the rgattr to attach
 * --------------------------------------------------------------------------------------------------------------------------------------------------------------         
 * -- @rgAttr_store_sub2__<methodname>__[<order:\d>]p<pi:\d>  
 *          as a attr for attach to a method param, like 
 *          ```
 *              static int? 
 *                  @rgAttr_store_sub2__x_on_ff__p0(StrCls state) => state.ACls?.I ?? 0;
 *              static int?
 *                  @rgAttr_store_sub2__x_on_ff__p1(StrCls state) => state.Msg;
 *              async Task _x_on_ff_(int i, object o) 
 *              { }
 *          ```
 *      order
 *          for the rgattrs order by
 *      pi
 *          for the the method param index
 * --------------------------------------------------------------------------------------------------------------------------------------------------------------  
 */

namespace rg
{
    using System;
    using System.Linq.Expressions;

    public static class SimpleRedux
    {
        public static void @sstobv<T, Tstate>(ref T p4rg, Func<Tstate, T> nolocalExpr4rg) => throw new InvalidOperationException("only for rg"); //Expression<Func<Tstate, T>>
        public static void @sstobv_<T, Tstate>(ref T p4rg, Func<Tstate, T> nolocalExpr4rg) => throw new InvalidOperationException("only for rg");

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class AutoSubscribe : Attribute
        {
            public AutoSubscribe() { }
            public AutoSubscribe(string type) { }
        }
    }
}