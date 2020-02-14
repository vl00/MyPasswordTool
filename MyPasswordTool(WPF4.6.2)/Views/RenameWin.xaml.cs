using Common;
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
using System.Windows.Interactivity;
using System.Windows.Media;

namespace MyPasswordTool.Views
{
    public partial class RenameWin : UserControl, IView<RenameViewModel>
    {
        public RenameWin()
        {
            InitializeComponent();
            //debug_test();

            this.KeyDown += RenameWin_KeyDown;
        }

        //public static WeakReference x;
        //public static object CMD;
        //public static WeakReference o;
        //public static WeakReference t;

        public RenameViewModel ViewModel => this.DataContext as RenameViewModel;

        //private void debug_test()
        //{
        //    this.Loaded += loaded;
        //    this.Unloaded += delegate
        //    {
        //        LogManager.GetLogger(this.GetType()).Info("RenameWin Unloaded");
        //        GC.Collect();
        //    };
        //}

        //private void loaded(object sender, RoutedEventArgs e)
        //{
        //    //GC.Collect();
        //    //t = new WeakReference(this);
        //    //o = new WeakReference(btn_ok);
        //    //CMD = this.ViewModel.RenameCmd;
        //    //x = new WeakReference(Interaction.GetTriggers(btn_ok)[0].Actions[0]);
        //}

        void RenameWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.IsDown)
            {
                ViewModel.RenameCmd.TryInvoke(new EventParamter { Sender = sender, EventArgs = e })
                    .TryThrow();
            }
        }
    }
}
