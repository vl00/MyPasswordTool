namespace Common
{
    public interface __IObj__
    {
        void __init__();
        void __cleanup__();
    }

    public static class __IObj__Ext__
    {
        public static void TryInit<T>(this T o) => (o as __IObj__)?.__init__();
        public static void TryClearup<T>(this T o) => (o as __IObj__)?.__cleanup__();
    }
}

//namespace eg
//{
//    using Common;

//    partial class eg : __IObj__
//    {
//        int __iC__;

//        //...

//        void __IObj__.__init__() { __init__(); }
//        void __IObj__.__cleanup__() { __cleanup__(); }

//        protected virtual void __init__()
//        {
//            var _ic_ = Interlocked.CompareExchange(ref __iC__, 1, 0);
//            if (_ic_ != 0) return;

//            //...
//        }

//        protected virtual void __cleanup__()
//        {
//            var _ic_ = Interlocked.CompareExchange(ref __iC__, 0, 1);
//            if (_ic_ != 1) return;

//            //...
//        }
//    }

//    partial class eg1 : eg
//    {
//        int __iC__;

//        //...

//        protected override void __init__()
//        {
//            base.__init__();

//            var _ic_ = Interlocked.CompareExchange(ref __iC__, 1, 0);
//            if (_ic_ != 0) return;

//            //...
//        }

//        protected override void __cleanup__()
//        {
//            base.__cleanup__();

//            var _ic_ = Interlocked.CompareExchange(ref __iC__, 0, 1);
//            if (_ic_ != 1) return;

//            //...
//        }
//    }
//}