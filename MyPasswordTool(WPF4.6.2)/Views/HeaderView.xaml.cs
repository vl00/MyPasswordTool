using Common;
using MyPasswordTool.Models;
using MyPasswordTool.ViewModels;
using Newtonsoft.Json.Linq;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static rg.SimpleRedux;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool
{
    public partial class HeaderView : UserControl, IView<MainViewModel>
    {
        public HeaderView()
        {
            InitializeComponent();
            this.Loaded += HeaderView_loaded;
            this.Unloaded += HeaderView_Unloaded;
            stc.KeyDown += on_KeyDown;
            _init();
        }

        public MainViewModel ViewModel => this.DataContext as MainViewModel;

        private void _init()
        {
            this.TryInit();
            on_ChangeSearchText((string)@state.JX?["searchtext"]);
        }

        void on_ChangeSearchText(string txt)
        {
            @sstobv(ref txt, (AppState _) => (string)_.JX?["searchtext"]);

            stc.Text = txt;
        }

        private void HeaderView_loaded(object sender, RoutedEventArgs e)
        {
            this.TryInit();
        }

        private void HeaderView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.TryClearup();
        }

        private void btn_lock_Click(object sender, RoutedEventArgs e)
        {
            @dispatch(new LockActionMessage { IsLock = true });
        }

        private void btn_add_click(object sender, RoutedEventArgs e)
        {
            btn_add.Focus();
            e.Handled = true;
            var c = btn_add.ContextMenu = this.FindStaticResource("cm_add") as ContextMenu;
            if (c == null) return;
            c.PlacementTarget = btn_add;
            c.DataContext = this.DataContext;
            AsyncHelper.RunInBatch(async () => 
            {
                await EventUtil.FromEventAsync<RoutedEventArgs>(
                    h => c.Closed += new RoutedEventHandler(h),
                    h => c.Closed -= new RoutedEventHandler(h),
                    () => c.IsOpen = true);
                btn_add.ContextMenu = null;
            });
        }

        void stc_OnSearch(object sender, object e)
        {
            ViewModel.SearchCmd.TryInvoke(new EventParamter { Sender = sender, EventArgs = e })
                .TryThrow();
        }

        void on_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.IsDown && stc.IsFocused)
            {
                stc_OnSearch(sender, e);
            }
        }
    }
}
