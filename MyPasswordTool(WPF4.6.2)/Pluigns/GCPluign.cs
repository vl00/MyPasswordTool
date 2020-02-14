using Common;
using SilverEx;
using System;
using System.Threading;

namespace MyPasswordTool
{
    public class GCPluign : MyPluign
    {
        readonly int t = 50 * 1000;
        int i;
        Timer gct;

        protected override void OnInit()
        {
            GcNotification.Register(on_gc, null);
            gct = new Timer(tick, null, t, Timeout.Infinite);
        }

        protected override void OnDispose()
        {
            gct.Dispose();
            gct = null;
        }

        private bool on_gc(object _)
        {
            if (++i < 2)
            {
                gct.Change(Timeout.Infinite, Timeout.Infinite); //stop
                gct.Change(t, Timeout.Infinite); //settimeout to run
            }
            return true;
        }

        private void tick(object _)
        {
            _gc();
        }

        private void _gc()
        {
            i = 0;
            GC.Collect();
            //GC.Collect(2, GCCollectionMode.Optimized, true);
            //LogManager.GetLogger(this.GetType()).Info("GC.Collect end");
        }
    }
}
