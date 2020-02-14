using Common;
using MyPasswordTool.ViewModels;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MyPasswordTool.Views
{
    public partial class TreepanelView : UserControl, IView<TagTreeViewModel>
    {
        public TreepanelView()
        {
            InitializeComponent(); 
        }

        public TagTreeViewModel ViewModel => this.DataContext as TagTreeViewModel;

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = e.OriginalSource.AsTo<DependencyObject>().FindSelfOrAncestor<TreeViewItem>();
            if (treeViewItem == null) return;
            treeViewItem.Focus(); //确保右键选上
            //treeViewItem.IsSelected = true;
            e.Handled = true;

            dynamic vm = treeViewItem.DataContext;
            var cm = "";
            if (vm.ID == null) cm = "cm_Trash";
            else if (vm.ID != null && vm.ID > 0) cm = "cm_selID";
            else if (vm.ID == -1) cm = "cm_all";
            if (!cm.IsNullOrEmpty())
            {
                var c = treeViewItem.ContextMenu = this.FindStaticResource(cm).AsTo<ContextMenu>();
                if (c == null) return;
                c.PlacementTarget = treeViewItem;
                c.DataContext = this.DataContext;
                c.Tag = vm;
            }
        }

        private void Expander_Click(object sender, RoutedEventArgs e)
        {
            var node = (sender as FrameworkElement).DataContext;
            ViewModel.ToggleCmd.TryInvoke(node);
        }
    }
}
