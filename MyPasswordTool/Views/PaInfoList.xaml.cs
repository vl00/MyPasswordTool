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
using static rg.SimpleRedux;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.Views
{
    public partial class PaInfoList : UserControl, IView<PaInfoListViewModel>
    {
        public PaInfoList()
        {
            InitializeComponent();
            init();
        }

        private ScrollViewer lb_sv;
        private readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();

        public PaInfoListViewModel ViewModel => this.DataContext as PaInfoListViewModel;

        async void init()
        {
            this.TryInit();

            if (!this.IsElementLoaded())
            {
                await EventUtil.FromEventAsync<RoutedEventArgs>(
                    h => this.Loaded += new RoutedEventHandler(h),
                    h => this.Loaded -= new RoutedEventHandler(h));
            }
            { //PaInfoList_Loaded
                lb_sv = lb.GetDescendants<ScrollViewer>().FirstOrDefault();
                _tcs.TrySetResult(null);
            }
        }

        [AutoSubscribe(Consts.DyActType.main_Activate)]
        async void Activate(object _)
        {
            this.TryInit();

            await _tcs.Task;
            if (lb_sv != null)
            {
                lb_sv.ScrollChanged -= lb_sv_ScrollChanged;
                lb_sv.ScrollChanged += lb_sv_ScrollChanged;
            }
        }

        [AutoSubscribe(Consts.DyActType.main_Clear)]
        void Clear(object _)
        {
            if (lb_sv != null) lb_sv.ScrollChanged -= lb_sv_ScrollChanged;

            this.TryClearup();
        }

        private void lb_sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ViewModel.Pas?.IsInitializing != false) return;
            if (e.HorizontalChange != 0 || e.VerticalChange != 0)
                @dispatch(new PaListScrollData { Hoff = lb_sv.HorizontalOffset, Voff = lb_sv.Content == null ? 0 : lb_sv.VerticalOffset });
        }

        [AutoSubscribe]
        async Task when_voff_noteq_svvoff(PaListScrollData action)
        {
            await _tcs.Task;
            var voff = action.Voff ?? 0;
            if (voff == 0) lb_sv.ScrollToTop(); //to use `lb_sv.ScrollToVerticalOffset(0);` will has bug
            else lb_sv.ScrollToVerticalOffset(voff);
        }

        private void btn_order_click(object sender, RoutedEventArgs e)
        {
            btn_order.Focus();
            e.Handled = true;
            var c = btn_order.ContextMenu = this.FindStaticResource("cm_order") as ContextMenu;
            if (c == null) return;
            c.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            c.PlacementTarget = btn_order;
            c.DataContext = this.DataContext;
            AsyncHelper.RunInBatch(async () =>
            {
                if (!c.IsElementLoaded())
                {
                    await EventUtil.FromEventAsync<RoutedEventArgs>(
                        h => c.Loaded += new RoutedEventHandler(h),
                        h => c.Loaded -= new RoutedEventHandler(h));
                }
                foreach (var _ci in c.GetDescendants<MenuItem>())
                {
                    if (ViewModel.Order.Equals(_ci.Tag)) _ci.IsChecked = true;
                    else _ci.IsChecked = false;
                }
            });
            AsyncHelper.RunInBatch(async () =>
            {
                await EventUtil.FromEventAsync<RoutedEventArgs>(
                    h => c.Closed += new RoutedEventHandler(h),
                    h => c.Closed -= new RoutedEventHandler(h));

                btn_order.ContextMenu = null;
            });
            c.IsOpen = true;
        }

        private void cm_order_Click(object sender, RoutedEventArgs e)
        {
            var ci = sender as MenuItem;
            var p = ci.Tag?.ToString();
            foreach (var _ci in (ci.Parent as ContextMenu).GetDescendants<MenuItem>())
            {
                _ci.IsChecked = false;
            }
            ci.IsChecked = true;

            @dispatch(Consts.DyActType.palsOrderChanged, p);
        }
    }
}
