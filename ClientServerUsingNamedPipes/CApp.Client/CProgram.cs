using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CApp_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Main_async().GetAwaiter().GetResult();
        }

        static Task Main_async()
        {
            var client = new Client("ttestsCApp", new NamedPipeClientConnectionFactory());
            var virtualClient = new VirtualConnectionClient(client);
            var virtualClient2 = new VirtualConnectionClient(client);

            Task.Factory.StartNew(async () =>
            {
                await virtualClient.ComunicationAsync(async virtualConnection =>
                {
                    Console.WriteLine($"0 f1 q");
                    virtualConnection.Connection.Close();
                    //await virtualConnection.WriteAsync(null);
                    var r = deserialize(await virtualConnection.ReadAsync());
                    Console.WriteLine($"0 f1 r={r}");
                });
            });
            //Task.Factory.StartNew(async () =>
            //{
            //    await virtualClient.ComunicationAsync(async virtualConnection =>
            //    {
            //        Console.WriteLine($"1 f1 q");
            //        await virtualConnection.WriteAsync(serialize(new { s = "f1" }));
            //        //virtualConnection.Connection.Close();
            //        //try { await virtualConnection.Connection.Open(); } catch { }
            //        await virtualConnection.WriteAsync(serialize(new { p1 = 1, p2 = "2" }));
            //        var r = deserialize(await virtualConnection.ReadAsync());
            //        Console.WriteLine($"1 f1 r={r}");
            //    });
            //});
            //Task.Factory.StartNew(async () =>
            //{
            //    await virtualClient2.ComunicationAsync(async virtualConnection =>
            //    {
            //        Console.WriteLine($"2 f1 q");
            //        await virtualConnection.WriteAsync(serialize(new { s = "f1" }));
            //        await virtualConnection.WriteAsync(serialize(new { p1 = 2, p2 = "2000" }));
            //        var r = deserialize(await virtualConnection.ReadAsync());
            //        Console.WriteLine($"2 f1 r={r}");
            //    });
            //});

            Console.ReadLine();
            virtualClient.Dispose();
            virtualClient2.Dispose();
            client.Dispose();

            Console.ReadLine();
            return Task.CompletedTask;
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
