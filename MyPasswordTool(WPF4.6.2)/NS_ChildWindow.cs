using Common;
using MyPasswordTool.Models;
using SilverEx;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static rg.SimpleRedux;

namespace MyPasswordTool
{
    partial class MainWindow
    {
        partial void init_ChildWindow()
        {
            ChildWindow.Host = ChildWinDislayArea;
        }

        void on_ChildWin(NavigationMessage msg)
        {
            @sstobv_(ref msg, (AppState _) => _.Action as NavigationMessage);

            if (msg.Type == Consts.NS.ChildWin)
                handle_ChildWindow_Navigation(msg.ViewKey, msg.Parameter);
        }

        async void handle_ChildWindow_Navigation(string key, object parameter)
        {
            var v = IoC.Resolve(key2Type(key), null) as FrameworkElement;
            if (v == null) throw new Exception(string.Format(Consts.Error.NotFindView));
            var vm = v.DataContext;

            //AsyncHelper.RunInBatch(async () =>
            //{
            //    await EventUtil.FromEventAsync<RoutedEventHandler, RoutedEventArgs>(
            //        h => new RoutedEventHandler(h),
            //        h => v.Unloaded += h,
            //        h => v.Unloaded -= h);
            //    LogManager.GetLogger(this.GetType()).Info("{0} unloaded", v.GetType());
            //    vm.TryClear();
            //});
            var cwin = v is ChildWindow ? v.AsTo<ChildWindow>() : new ChildWindow() { Content = v };
            cwin.Show();
            //if (vm != null) vm.Init();

            if (!v.IsElementLoaded())
            {
                await EventUtil.FromEventAsync<RoutedEventArgs>(
                        h => v.Loaded += new RoutedEventHandler(h),
                        h => v.Loaded -= new RoutedEventHandler(h)
                    );
            }
            LogManager.GetLogger(this.GetType()).Info("{0} loaded", v.GetType());
            (vm as IMvvmAware)?.Activate(parameter);

            var bcancel = false;
            do
            {
                var e = await EventUtil.FromEventAsync<CancelEventArgs>(
                        h => cwin.Closing += h,
                        h => cwin.Closing -= h);

                var args = new NotifyBag();
                vm.AsTo<IMvvmAware>()?.Deactivate(args);
                bcancel = e.Cancel = args.Get("Cancel").CastTo<bool?>() ?? false;

            } while (bcancel);
        }
    }
}
