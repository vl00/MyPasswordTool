using Common;
using MyPasswordTool.Models;
using Newtonsoft.Json;
using SilverEx;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool
{
    public partial class MyBootstrapper
    {
        private readonly IList<IDisposable> dpool = new List<IDisposable>();

        public IEnumerable<Lazy<object>> MyParts { get; set; }

        public ILogger Logger { get; set; } = LogManager.GetLogger(null);

        partial void map_view_viewmodel(IDictionary<Type, Type> mapper);

        partial void OnPrepareApplication()
        {
            JsonNetExtensions.SerializerSettings.Converters.Add(new NotifyBagJsonConverter());
            LogManager.GetLogger = type => Logger;

            AppExt.InitSqlite3();
        }

        protected override void Configure()
        {
            base.Configure();
            _ioc_Configure();

            MvvmUtil.GetObjectByType = (type) => IoC.Resolve(type, null);
            MyFrame.CreatePageFunc = type => (MyPage)IoC.Resolve(type, null);

            MyParts = IoC.ResolveAll<Lazy<MyPluign>>().ToArray().Select(p => new Lazy<object>(() => p.Value));

            DelegateCommand.ConverParameter = (o, type) =>
            {
                object v;
                ConverterHelper.TryConvert(o, type, out v);
                return v;
            };

            map_view_viewmodel(MvvmUtil.Mapper);
        }

        partial void OnAppStartup(StartupEventArgs e) 
        {
            init_myparts();
            this.TryInit();

            var rootFrame = createShellFrame();
            if (rootFrame.Content == null)
            {
                rootFrame.NavigateAsync(IoC.Resolve<Consts.MapKeyToViewType>().Invoke(Consts.ViewKey.Palock));
            }
            ShowRootUI(rootFrame);
            Application.Current.MainWindow.Activate();
        }

        partial void OnAppExit(ExitEventArgs e) 
        {
            try 
            {
                foreach (var d in dpool)
                    d?.Dispose();

                var dbinfo = IoC.Resolve<DbConnInfo>();
                var bt = @dispatchAsync(new BackgroundMessage
                {
                    Token = Consts.app_exit,
                    //IsDirect = true,
                    Message = new
                    {
                        db = dbinfo?.DB,
                        pwd = dbinfo?.Pwd,
                    }
                });
               (bt ?? Task.CompletedTask).Wait(TimeSpan.FromMilliseconds(Consts.ExitWaitMilliseconds)); 
            }
            finally
            {
                foreach (var d in MyParts)
                    (d.Value as IDisposable)?.Dispose();

                this.TryClearup();
            }
        }

        async partial void OnAppUnhandledException(DispatcherUnhandledExceptionEventArgs e) 
        {
            //e.Handled = true;
            await ThreadUtil.CallOnUIThread(() => Logger.Error(e.Exception));
        }

        private void init_myparts()
        {
            foreach (var part in MyParts)
            {
                try
                {
                    var v = part.Value as MyPluign;
                    v?.Init();
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(this.GetType()).Error(ex);
                }
            }
        }

        protected void ShowRootUI(FrameworkElement ui) => IoC.Resolve<MainWindow>().ShowUI(ui);

        private MyFrame createShellFrame()
        {
            var f = new MyFrame();

            //SuspensionManager.RegisterFrame(f, "AppFrame"); //    
            f.CacheSize = 2;
            f.MaxBackJournalsCount = 1;

            f.Navigating += frame_Navigating;
            f.Navigated += frame_Navigated;
            //f.NavigationFailed += frame_NavigationFailed;
            //f.NavigationStopped += frame_NavigationStopped;

            f.ContentTemplate = Application.Current.Resources["shellFrameContentTmp"] as DataTemplate;

            return f;
        }

        private void frame_Navigating(object sender, MyNavigatingCancelEventArgs e)
        {
            //Logger.Info("shell frame_Navigating");
        }

        private void frame_Navigated(object sender, MyNavigationEventArgs e)
        {
            //Logger.Info("shell frame_Navigated");
        }

        //private void frame_NavigationFailed(object sender, MyNavigationFailedEventArgs e)
        //{
        //    throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        //}

        private static TransitioningContentControl getTransitioningContentControl(MyFrame frame)
        {
            return frame.GetDescendants<TransitioningContentControl>().FirstOrDefault();
        }
    }
}
