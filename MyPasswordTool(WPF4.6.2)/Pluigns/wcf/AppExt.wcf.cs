using Common;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace MyPasswordTool
{
    public static partial class AppExt
    {
        public static async Task InvokeWCF<T>(string endpointConfigurationName, Func<T, Task> func)
        {
            var f = new ChannelFactory<T>(endpointConfigurationName);
            var cn = f.CreateChannel();
            try
            {
                await (func(cn) ?? Task.CompletedTask);
            }
            catch
            {
                throw;
            }
            finally
            {
                wcfSafeClose(cn);
                wcfSafeClose(f);
            }
        }

        public static async Task<TR> InvokeWCF<T, TR>(string endpointConfigurationName, Func<T, Task<TR>> func)
        {
            var f = new ChannelFactory<T>(endpointConfigurationName);
            var cn = f.CreateChannel();
            try
            {
                return await func(cn);
            }
            catch
            {
                throw;
            }
            finally
            {
                wcfSafeClose(cn);
                wcfSafeClose(f);
            }
        }

        private static void wcfSafeClose<T>(T proxy)
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
