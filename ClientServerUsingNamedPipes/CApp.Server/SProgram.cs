using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CApp_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Main_async().GetAwaiter().GetResult();
        }

        async static Task Main_async()
        {
            var server = new Server("ttestsCApp", SilverEx.LogManager.GetLogger(null), new NamedPipeServerConnectionFactory());
            server.Start();
            server.OnClientConnected += (connection) =>
            {
                Console.WriteLine($"conn {connection.Id} opened");
            };
            server.OnClientDisconnected += (connection) => 
            {
                Console.WriteLine($"conn {connection.Id} closed");
            };

            VirtualConnectionServer.At(server)(async virtualConnection =>
            {
                var s = deserialize(await virtualConnection.ReadAsync())?["s"]?.ToString();
                var p = deserialize(await virtualConnection.ReadAsync());
                switch(s)
                {
                    case "f1":
                        {
                            var r = await f1((int)p["p1"], (string)p["p2"]);
                            await virtualConnection.WriteAsync(serialize(r));
                        }
                        break;
                }
            });

            await Task.CompletedTask;
            Console.ReadLine();

            async ValueTask<int> f1(int p1, string p2)
            {
                await Task.CompletedTask;
                Console.WriteLine($"f1 p1={p1} p2=`{p2}`");
                return p1;
            }
        }

        static byte[] serialize(object o)
        {
            var json = JsonConvert.SerializeObject(o);
            return Encoding.UTF8.GetBytes(json);
        }

        static JToken deserialize(byte[] buffer)
        {
            return JToken.Parse(Encoding.UTF8.GetString(buffer));
        }
    }
}
