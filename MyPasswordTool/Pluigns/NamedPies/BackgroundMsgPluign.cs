using Common;
using MyPasswordTool.Models;
using Newtonsoft.Json;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static rg.SimpleRedux;

namespace MyPasswordTool
{
    public partial class BackgroundMsgPluign : MyPluign
    {
        Client client;
        VirtualConnectionClient virtualClient;

        protected override void OnInit()
        {
            this.TryInit();

            client = new Client("MyPasswordTool", new NamedPipeClientConnectionFactory());
            virtualClient = new VirtualConnectionClient(client);
        }

        protected override void OnDispose() 
        {
            this.TryClearup();

            virtualClient?.Dispose();
            client?.Dispose();
        }

        [AutoSubscribe]
        async Task _work(BackgroundMessage msg)
        {
            Task _()
            {
                return virtualClient.ComunicationAsync(async virtualConnection =>
                {
                    using (virtualConnection)
                    {
                        await virtualConnection.WriteAsync(serialize(new { s = "BackgroundService", a = "DoWork" }));
                        await virtualConnection.WriteAsync(serialize(msg));
                    }
                });
            }

            if (msg == null) return;
            try
            {
                if (msg.IsDirect) await _(); 
                else await Task.Run(_);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex);
            }
        }

        static byte[] serialize(object o)
        {
            var json = o.ToJson();
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
