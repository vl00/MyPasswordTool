using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MyPasswordTool.Service
{
    public class NamedPies_BackgroundService : IBootstartup
    {
        SilverEx.ILogger logger = SilverEx.LogManager.GetLogger(typeof(NamedPies_BackgroundService));
        Server server;

        void IBootstartup.Init()
        {
            server = new Server("MyPasswordTool", logger, new NamedPipeServerConnectionFactory());
            
            server.OnClientConnected += (connection) =>
            {
                logger.Info($"conn {connection.Id} opened");
            };
            server.OnClientDisconnected += (connection) =>
            {
                logger.Info($"conn {connection.Id} closed");
            };

            server.Start();
            logger.Info("BackgroundService server opened");

            VirtualConnectionServer.At(server)(on_virtualConnection);
        }

        void IDisposable.Dispose()
        {
            server.Dispose();
            logger.Info("BackgroundService server closed");
        }

        async void on_virtualConnection(VirtualConnection virtualConnection)
        {
            using (virtualConnection)
            {
                var jsa = deserialize(await virtualConnection.ReadAsync());
                var s = jsa?["s"]?.ToString();
                var a = jsa?["a"]?.ToString();
                var p = Encoding.UTF8.GetString(await virtualConnection.ReadAsync());

                var r = do_s(s, a, p);
                if (r != null) await virtualConnection.WriteAsync(serialize(await r));
            }
        }

        static byte[] serialize(object o) => Encoding.UTF8.GetBytes(o.ToJson());
        static JToken deserialize(byte[] buffer) => buffer == null ? null : JToken.Parse(Encoding.UTF8.GetString(buffer));

        Task<object> do_s(string s, string a, string p)
        {
            switch (s)
            {
                case nameof(BackgroundService):
                    {
                        var so = new BackgroundService();
                        return do_s_a(so, s, a, p);
                    }
                default:
                    logger.Warn($"not find s={s} a={a}");
                    return null;
            }
        }

        Task<object> do_s_a(BackgroundService so, string s, string a, string p)
        {
            switch (a)
            {
                case nameof(so.DoWork):
                    {
                        so.DoWork(p);
                        return null;
                    }
                default:
                    logger.Warn($"not find s={s} a={a}");
                    return null;
            }
        }
    }
}
