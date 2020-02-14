using Common;
using MyPasswordTool.Models;
using MyPasswordTool.ViewModels;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using static rg.SimpleRedux;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool
{
    public partial class MainPage : MvvmAwarePage, IView<MainViewModel>, IClear
    {
        public MainPage()
        {
            InitializeComponent(); //too busy???
            init();
        }

        private Consts.MapKeyToViewType mapKeyToViewType { get; } = IoC.Resolve<Consts.MapKeyToViewType>();

        public MainViewModel ViewModel => this.DataContext as MainViewModel;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //VisualStateManager.GoToState(this, "vs_lock", false);
        }

        private void init() => this.TryInit();

        protected override void OnNavigatedTo(MyNavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LockStateChanged(e.Parameter as LockActionMessage);
        }

        protected override void OnNavigatedFrom(MyNavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            //LockStateChanged(e.Parameter as LockActionMessage);
            if ((e.Parameter as LockActionMessage)?.IsLock == true)
            {
                VisualStateManager.GoToState(this, "vs_lock", false);
            }
        }

        [AutoSubscribe(Consts.DyActType.before_unlock_ok)]
        void at_before_unlock_ok((bool is_first_unlock, string new_conn, string old_conn) _)
        {
            if (!_.is_first_unlock && _.new_conn != _.old_conn)
            {
                LogManager.GetLogger(this.GetType()).Info("action unlock effect clear old MainPage");
                (this as IClear)?.Clear();
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        void _when_lock(LockActionMessage act)
        {
            @sstobv_(ref act, (AppState _) => _.Action as LockActionMessage);

            if (act.IsLock)
            {
                LogManager.GetLogger(this.GetType()).Info("action lock effect nav back PalockView");
                LockStateChanged(act);
            }
        }

        async Task _on_nav(NavigationMessage act)
        {
            @sstobv_(ref act, (AppState _) => _.Action as NavigationMessage);

            var frame = div_content;
            if (!frame.IsElementLoaded())
            {
                //return;
                await EventUtil.FromEventAsync<RoutedEventArgs>(
                        h => frame.Loaded += new RoutedEventHandler(h),
                        h => frame.Loaded -= new RoutedEventHandler(h)
                    );
            }
            var tcc = frame.GetDescendants<TransitioningContentControl>().First();
            switch (act.Type)
            {
                case Consts.NS.BeforeDuc:
                case Consts.NS.EmptyDuc:
                    tcc.Transition = TransitionType.Normal;
                    frame.Journals.Clear();
                    frame.Content = null;
                    break;
                case Consts.NS.DucTrash:
                    tcc.Transition = TransitionType.Default;
                    frame.MaxBackJournalsCount = 0;
                    _ = frame.NavigateAsync(mapKeyToViewType(act.ViewKey), act.Parameter);
                    break;
                case Consts.NS.Duc:
                    tcc.Transition = TransitionType.Default;
                    frame.MaxBackJournalsCount = 0;
                    _ = frame.NavigateAsync(mapKeyToViewType(act.ViewKey), act.Parameter);
                    break;
                case Consts.NS.Edit:
                    tcc.Transition = TransitionType.Left;
                    frame.MaxBackJournalsCount = 1;
                    _ = frame.NavigateAsync(mapKeyToViewType(act.ViewKey), act.Parameter);
                    break;
                case Consts.NS.Edit_BackTo_Duc:
                    tcc.Transition = TransitionType.Right;
                    await frame.GoBackAsync(act.Parameter);
                    frame.MaxBackJournalsCount = 0;
                    break;
            }
        }

        private async void LockStateChanged(LockActionMessage act)
        {
            if (act == null) return;
            if (!act.IsLock)
            {
                VisualStateManager.GoToState(this, "vs_unlock", false);
            }
            else
            {
                //VisualStateManager.GoToState(this, "vs_lock", false);
                //await AsyncHelper.RunAsTask(async ok =>
                //{
                //    var g = VisualStateManager.GetVisualStateGroups(grid)
                //        .OfType<VisualStateGroup>()
                //        .First(group => string.CompareOrdinal("LockStateGroup", @group.Name) == 0);

                //    var s = g.States.OfType<VisualState>().First(state => state.Name == "vs_lock");

                //    var t = EventUtil.FromEventAsync<EventHandler, EventArgs>(
                //            h => (o, e) => h(o, e),
                //            h => s.Storyboard.Completed += h,
                //            h => s.Storyboard.Completed -= h
                //        );

                //    VisualStateManager.GoToState(this, "vs_lock", false);
                //    await t;
                //    ok();
                //});
                await Frame.GoBackAsync(act);
            }
        }

        void IClear.Clear()
        {
            this.NavigationCacheMode = System.Windows.Navigation.NavigationCacheMode.Disabled;
            (ViewModel as IClear)?.Clear();
            this.TryClearup();
        }
    }
}
