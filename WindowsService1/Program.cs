using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService1.ServiceReference1 
{
    [ServiceContract]
    public interface IBackgroundService
    {

        [OperationContract(IsOneWay = true, Action = "http://tempuri.org/IBackgroundService/DoWork")]
        //[OperationContract(IsOneWay = true)]
        void DoWork(string args);

        //[OperationContract(Action = "http://tempuri.org/IBackgroundService/DoWorkWithResult", ReplyAction = "http://tempuri.org/IBackgroundService/DoWorkWithResultResponse")]
        [OperationContract]
        Task<string> DoWorkWithResultAsync(string args);
    }
}

namespace WindowsService1
{
    public class BackgroundMessage
    {
        public dynamic Message { get; set; }
        public string Token { get; set; }
        public string DB { get; set; }
    }

    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new Service1() 
            //};
            //ServiceBase.Run(ServicesToRun);

            f2();
            Console.ReadLine();
        }

        static async void f()
        {
            var client = new ChannelFactory<WindowsService1.ServiceReference1.IBackgroundService>("ServiceReference1.IBackgroundService");
            var cn = client.CreateChannel();
            try
            {
                //Console.WriteLine(await client.DoWorkWithResultAsync("asdddddddd"));
                cn.DoWork(JsonConvert.SerializeObject(new BackgroundMessage 
                {
                    DB = "dsad",
                    Token = "test",
                    Message = 188,
                }));
            }
            finally
            {
                client.Close();
            }
        }

        static void f2()
        {
            Class2.ExecBatCommand(p => 
            {
                p(@"node 1.js");
                //p("exit 0");
            });
        }
    }
}
