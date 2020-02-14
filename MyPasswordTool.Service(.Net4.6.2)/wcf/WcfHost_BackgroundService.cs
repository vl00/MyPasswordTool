using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MyPasswordTool.Service
{
    public class WcfHost_BackgroundService : IBootstartup
    {
        ServiceHost host;
        SilverEx.ILogger logger = SilverEx.LogManager.GetLogger(typeof(WcfHost_BackgroundService));

        void IBootstartup.Init()
        {
            host = new ServiceHost(typeof(BackgroundService));
            host.Opened += delegate { logger.Info("BackgroundService server opened"); };
            host.Closed += delegate { logger.Info("BackgroundService server closed"); };
            host.Open();
        }

        void IDisposable.Dispose()
        {
            wcfSafeClose(host);
        }

        static void wcfSafeClose<T>(T proxy)
        {
            var comm = proxy as ICommunicationObject;
            if (comm == null) return;
            try
            {
                if (comm.State != CommunicationState.Faulted) comm.Close();
                else comm.Abort();
            }
            catch
            {
                comm.Abort();
            }
        }
    }
}
