using Microsoft.VisualBasic.ApplicationServices;
using System;

namespace SilverEx
{
    public class AppProgramController
    {
        private AppProgramController()
        {
            this.c = new AppProgramController_ApplicationBase();
        }

        private readonly AppProgramController_ApplicationBase c;

        public static AppProgramController WithInstance()
        {
            return new AppProgramController();
        }

        public AppProgramController WhenFirstStartup(Action action)
        {
            c.Startup += (o, e) => action();
            return this;
        }

        public AppProgramController WhenFirstStartup(EventHandler<StartupEventArgs> handler)
        {
            c.Startup += (o, e) => handler(o, e);
            return this;
        }

        public AppProgramController WhenNextStartup(Action action)
        {
            c.StartupNextInstance += (o, e) => action();
            return this;
        }

        public AppProgramController WhenNextStartup(EventHandler<StartupNextInstanceEventArgs> handler)
        {
            c.StartupNextInstance += (o, e) => handler(o, e);
            return this;
        }

        public void RunAsSingle()
        {
            c.IsForSingleInstance = true;
            this.Run();
        }

        public void Run()
        {
            c.Run(Environment.GetCommandLineArgs());
        }

        private class AppProgramController_ApplicationBase : WindowsFormsApplicationBase
        {
            public bool IsForSingleInstance
            {
                get { return base.IsSingleInstance; }
                set { base.IsSingleInstance = value; }
            }

            protected override bool OnStartup(StartupEventArgs e)
            {
                base.OnStartup(e);
                return false;
            }
        }
    }
}