using SilverEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MyPasswordTool
{
    public partial class MyBootstrapper : Bootstrapper
    {
        public MyBootstrapper()
        {
            this.Initialise();
        }

        protected override void StartOnDesignTime() 
        {
            PrepareApplication();
        }

        protected override void StartOnRuntime()
        {
            PrepareApplication();
        }

        protected override void PrepareApplication()
        {
            Application.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            Application.Startup += Application_Startup;
            Application.Exit += Application_Exit;
            OnPrepareApplication();
        }

        partial void OnPrepareApplication();
        partial void OnAppStartup(StartupEventArgs e);
        partial void OnAppExit(ExitEventArgs e);
        partial void OnAppUnhandledException(DispatcherUnhandledExceptionEventArgs e);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ThreadUtil.InitializeThreadDispatcher();
            Configure();
            OnAppStartup(e);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            OnAppExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            OnAppUnhandledException(e);
        }
    }
}
