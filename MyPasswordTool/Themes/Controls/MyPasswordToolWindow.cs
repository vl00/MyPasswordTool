using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MyPasswordTool
{
    public class MyPasswordToolWindow : Window
    {
        //static MyPasswordToolWindow()
        //{
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(MyPasswordToolWindow), new FrameworkPropertyMetadata(typeof(MyPasswordToolWindow)));
        //}

        public MyPasswordToolWindow()
        {
            this.DefaultStyleKey = typeof(MyPasswordToolWindow);

            var showSysMenu = new CommandBinding(SystemCommands.ShowSystemMenuCommand, OnShowSystemMenuCommand);
            this.CommandBindings.Add(showSysMenu);

            var closeWindow = new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindowCommand);
            this.CommandBindings.Add(closeWindow);

            var maxWindow = new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindowCommand);
            var restoreWindow = new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindowCommand);
            this.CommandBindings.Add(maxWindow);
            this.CommandBindings.Add(restoreWindow);

            var minWindow = new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindowCommand);
            this.CommandBindings.Add(minWindow);

            if (this.ResizeMode != ResizeMode.NoResize || this.ResizeMode != ResizeMode.CanMinimize)
                FullScreenManager.RepairWpfWindowFullScreenBehavior(this);

            if (bool.Parse(ConfigurationManager.AppSettings["allowsTransparency"].ToString()))
            {
                this.AllowsTransparency = true;
                this.WindowStyle = WindowStyle.None;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var border = GetTemplateChild("WindowBorder") as Border;
            if (border != null)
            {
                var backgroundAnimation = border.Resources["BackgroundAnimation"] as Storyboard;
                if (backgroundAnimation != null)
                    backgroundAnimation.Begin();
            }
        }

        private void OnMinimizeWindowCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var w = Window.GetWindow(this);
            SystemCommands.MinimizeWindow(this);
        }

        private void MaxOrRestoreWindow()
        {
            var w = Window.GetWindow(this);
            //Action<Window> act = w.WindowState == System.Windows.WindowState.Maximized ? (w)=> SystemCommands.RestoreWindow(w) : (w)=>SystemCommands.MaximizeWindow(w);
            if (w.WindowState == System.Windows.WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(w);
            }
            else
            {
                SystemCommands.MaximizeWindow(w);
            }
        }

        private void OnRestoreWindowCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.MaxOrRestoreWindow();
        }

        private void OnMaximizeWindowCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.MaxOrRestoreWindow();
        }

        private void OnCloseWindowCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var w = Window.GetWindow(this);
            SystemCommands.CloseWindow(w);
        }

        private void OnShowSystemMenuCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var w = Window.GetWindow(this);
            var p = new Point(w.Left + 24, w.Top + 24);
            SystemCommands.ShowSystemMenu(w, p);
        }
    }
}
