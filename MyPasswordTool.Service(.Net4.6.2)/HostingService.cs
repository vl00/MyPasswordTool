using System.Collections.Generic;
using System.Linq;

namespace MyPasswordTool.Service
{
    static class HostingService
    {
        public static readonly IList<IBootstartup> Bootstartups = new List<IBootstartup>();

        public static void Start()
        {
            foreach (var bs in Bootstartups)
                bs?.Init();
        }

        public static void Stop()
        {
            foreach (var bs in Bootstartups.Reverse())
                bs?.Dispose();
        }
    }
}