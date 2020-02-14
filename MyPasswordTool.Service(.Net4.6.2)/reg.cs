using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator()]

namespace MyPasswordTool.Service
{
    partial class Program
    {
        private static void Reg()
        {
            SilverEx.LogManager.GetLogger = type => new log4netLogger(type);

            SilverEx.IoC.SetResolve(delegate { return null; });
            SilverEx.IoC.SetResolveAll(delegate { return Enumerable.Empty<object>(); });

            HostingService.Bootstartups.Add(new Sqlite3_Bootstartup());

            //HostingService.Bootstartups.Add(new WcfHost_BackgroundService());
            HostingService.Bootstartups.Add(new NamedPies_BackgroundService());
        }
    }
}