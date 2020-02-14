using Common;
using MyPasswordTool.Models;
using MyPasswordTool.WCFService;
using Newtonsoft.Json;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MyPasswordTool.AppExt;
using static rg.SimpleRedux;

namespace MyPasswordTool
{
    public partial class BackgroundMsgPluign : MyPluign
    {
        protected override void OnInit()
        {
            this.TryInit();
        }

        protected override void OnDispose() 
        {
            this.TryClearup();
        }

        [AutoSubscribe]
        async void _work(BackgroundMessage msg)
        {
            if (msg == null) return;
            try
            {
                var t = InvokeWCF<IBackgroundService>("BackgroundService", bs =>
                {
                    if (msg.IsDirect)
                    {
                        bs.DoWork(msg.ToJson());
                        return Task.CompletedTask;
                    }
                    else
                    {
                        return Task.Run(() => bs.DoWork(msg.ToJson()));
                    }
                });
                await t.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex);
            }
        }
    }
}
