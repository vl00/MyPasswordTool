using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace MyPasswordTool.Service
{
    partial class Program
    {
        private const string currentProcessName = "MyPasswordTool.Service";
        private static readonly double exitDelay = (double)Convert.ChangeType(ConfigurationManager.AppSettings["exitDelay"], typeof(double), null);

        public static readonly ManualResetEventSlim Waiter = new ManualResetEventSlim(false);

        [STAThread]
        static void Main(string[] args)
        {
            _run(args);
            Waiter.Wait();
        }

        private static void _run(string[] args)
        {
#if !DEBUG
            if (Process.GetProcessesByName(currentProcessName).Length > 1)
            {
                Waiter.Set();
                return;
            }
            if (CheckAdministrator())
#endif
            {
                Reg();
                console_main();
            }
        }

        private static void console_main()
        {
            HostingService.Start();

            Waiter.Wait();

            HostingService.Stop();
        }

        public static bool CheckAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);
            var runAsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);

            if (!runAsAdmin)
            {
                // It is not possible to launch a ClickOnce app as administrator directly,  
                // so instead we launch the app as administrator in a new process.  
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);
                // The following properties run the new process as administrator  
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";

                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    SilverEx.LogManager.GetLogger(typeof(Program)).Error(ex);
                }

                // Shut down the current process  
                Environment.Exit(0);
                //Console.ReadLine();
            }

            return runAsAdmin;
        }

        public static async void CheckCanExit(Action cando)
        {
            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(exitDelay)).ConfigureAwait(false);
                var count = Process.GetProcessesByName("MyPasswordTool")?.Length ?? 0;
                if (count == 0)
                {
                    cando?.Invoke();
                    Waiter.Set();
                    return;
                }
                else if (count == 1) continue;
                else break;
            } while (true);
        }
    }
}