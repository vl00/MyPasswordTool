using Common;
using Microsoft.Win32;
using MyPasswordTool.Models;
using MyPasswordTool.ViewModels;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static rg.SimpleRedux;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool.Views
{
    public partial class PalockView : MvvmAwarePage, IView<PalockViewModel>
    {
        public PalockView()
        {
            is_firstInstance = is_firstInstance == null ? true : false;
            InitializeComponent();
            this.Loaded += PalockView_Loaded;
        }

        private static bool? is_firstInstance;
        private Action disposeAction = delegate { };

        public PalockViewModel ViewModel => this.DataContext as PalockViewModel;

        protected override void OnNavigatedTo(MyNavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.KeyDown += on_KeyDown;
            this.TryInit();

            var _d_pwd = ViewModel.ListenPropertyChanged(nameof(ViewModel.Pwd), _ =>
            {
                if (!Equals(txt_pwd.Password, ViewModel.Pwd))
                    txt_pwd.Password = ViewModel.Pwd;
            });
            disposeAction += () => _d_pwd.Dispose();
            txt_pwd.Password = ViewModel.Pwd;

            LockStateChanged(e.Parameter as LockActionMessage);
        }

        void PalockView_Loaded(object sender, RoutedEventArgs e)
        {
            txt_pwd.Focus();
        }

        protected override void OnNavigatedFrom(MyNavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.KeyDown -= on_KeyDown;

            if ((e.Parameter as LockActionMessage)?.IsLock == false)
            {
                VisualStateManager.GoToState(this, "state_unlock", false);
            }

            this.TryClearup();
            (ViewModel as IClear)?.Clear();
            disposeAction();
            disposeAction = null;
        }

        void on_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.IsDown)
            {
                ViewModel.OkCmd.TryInvoke(new EventParamter { Sender = sender, EventArgs = e, CommandParameter = gd.Tag })
                    .TryThrow();
            }
        }

        void rg_when_lockaction(LockActionMessage act)
        {
            @sstobv_(ref act, (AppState _) => (_.Action as LockActionMessage));
            //@sstobv(ref b, (AppState _) => ((string)_.JX["db"]) != _.prevdb);

            if (!act.IsLock)
            {
                var newnav = is_firstInstance == true || ((string)@state.JX["db"]) != @state.prevdb;
                LogManager.GetLogger(GetType()).Info($"action unlock effect nav to {(newnav ? "new" : "old")} MainPage");
                LockStateChanged(act, newnav);
            }
        }

        private async void LockStateChanged(LockActionMessage act, bool newNav = true)
        {
            if (act == null) return;
            if (!act.IsLock)
            {
                Task t_nav = null;
                if (newNav) t_nav = Frame.NavigateAsync(IoC.Resolve<Consts.MapKeyToViewType>()(Consts.ViewKey.Main), act); 
                else t_nav = Frame.GoForwardAsync(act);
                await t_nav;
            }
            else
            {
                VisualStateManager.GoToState(this, "state_lock", false);
            }
        }

        private void btn_openDB_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            //ofd.InitialDirectory = @"D:\";
            ofd.Filter = "文件(*.db)|*.db|所有文件(*.*)|*.*";
            if (ofd.ShowDialog() == true)
            {
                txt_db.Text = ofd.FileName;
                gd.Tag = "openDB";
            }
        }

        private void btn_newDB_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "文件(*.db)|*.db";
            if (sfd.ShowDialog() == true)
            {
                txt_db.Text = sfd.FileName;
                gd.Tag = "newDB";
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Pwd = txt_pwd.Password;
        }
    }
}
