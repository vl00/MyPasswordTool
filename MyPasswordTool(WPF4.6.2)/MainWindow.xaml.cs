using Common;
using MyPasswordTool.Models;
using SilverEx;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace MyPasswordTool
{
    public partial class MainWindow : MyPasswordToolWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.TryInit();
            init_ChildWindow();
        }

        partial void init_ChildWindow();
        private readonly Consts.MapKeyToViewType key2Type = IoC.Resolve<Consts.MapKeyToViewType>();
        private readonly Consts.MapViewTypeToKey type2Key = IoC.Resolve<Consts.MapViewTypeToKey>();

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //if (this.TaskbarItemInfo != null)
            //{
            //    this.TaskbarItemInfo.Overlay = (ImageSource)Application.Current.Resources["appIcon"];
            //}
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.TryClearup();
            base.OnClosing(e);
        }

        public void ShowUI(FrameworkElement ui)
        {
            contentPresenter.Content = ui;
            this.Show();
        }
    }
}
