using SilverEx;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Common.ObjectHelper;

namespace MyPasswordTool
{
    public partial class MyBootstrapper
    {
        [STAThread]
        public static void Main()
        {
            _main();
        }

        private static Application frun()
        {
            Process p = null;
#if !DEBUG
            if (!CheckAdministrator())
            {
                try
                {
                    var psi = new ProcessStartInfo();
                    psi.FileName = typeof(MyBootstrapper).Assembly.ManifestModule.FullyQualifiedName;
                    psi.UseShellExecute = true;
                    psi.Verb = "runas";
                    p = Process.Start(psi);  //uac
                }
                catch { }
                return null;
            }
#endif
            p = Process.GetProcessesByName("MyPasswordTool.Service").ElementAtOrDefault(0);
            if (p == null)
            {
                try
                {
                    var psi = new ProcessStartInfo();
                    psi.FileName = Path.Combine(Directory.GetCurrentDirectory(), "MyPasswordTool.Service.exe");
                    //psi.Arguments = Process.GetCurrentProcess().Id.ToString();
                    psi.UseShellExecute = true;
                    //psi.Verb = "runas"; //uac
                    psi.CreateNoWindow = false;
                    p = Process.Start(psi); 
                }
                catch
                {
                    return null;
                }
            }

            var app = new App();
            var bootstrapper = app.Bootstrapper = new MyBootstrapper();
            bootstrapper.Initialise();
            app.InitializeComponent();
            return app;
        }

        private static void _main()
        {
            var b = ConfigurationManager.AppSettings["singleable"].CastTo<bool>();
            if (!b)
            {
                frun()?.Run();
            }
            else
            {
                try
                {
                    AppProgramController.WithInstance()
                        .WhenFirstStartup(() => frun()?.Run())
                        .WhenNextStartup((_, e) =>
                        {
                            var win = Application.Current.MainWindow;
                            if (win == null) return;
                            win.Show();
                            if (win.WindowState == WindowState.Minimized) win.WindowState = WindowState.Normal;
                            win.Activate();
                        })
                        .RunAsSingle();
                }
                catch
                {
                    //frun()?.Run();
                    throw;
                }
            }
        }

        private static bool CheckAdministrator() => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
