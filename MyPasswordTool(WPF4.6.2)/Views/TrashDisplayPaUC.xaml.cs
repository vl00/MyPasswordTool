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

namespace MyPasswordTool.Views
{
    public partial class TrashDisplayPaUC : MvvmAwarePage, IView<DucTrashViewModel>
    {
        public TrashDisplayPaUC()
        {
            InitializeComponent();

            txt_time.DataContextChanged += delegate
            {
                var pa = txt_time.DataContext.AsDynamic();
                if (pa == null) return;
                txt_time.Text = string.Format("建于{0:yyyy-MM-dd HH:mm:ss} || 最后修改{1:yyyy-MM-dd HH:mm:ss}", pa.CreateTime, pa.UpdateTime);
            };
        }

        public DucTrashViewModel ViewModel
        {
            get { return this.DataContext as DucTrashViewModel; }
        }
    }
}
