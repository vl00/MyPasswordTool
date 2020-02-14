using Common;
using Microsoft.Win32;
using MyPasswordTool.ViewModels;
using SilverEx;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyPasswordTool.Views
{
    public partial class PageDuc0 : MvvmAwarePage, IView<Duc0ViewModel>
    {
        public PageDuc0()
        {
            InitializeComponent();
        }

        public Duc0ViewModel ViewModel
        {
            get { return this.DataContext as Duc0ViewModel; }
        }

        protected override void OnNavigatedTo(MyNavigationEventArgs e)
        {
            {
                IDisposable d = null;
                d = ViewModel.ListenPropertyChanged("PaInfo", _ =>
                {
                    d?.Dispose();
                    dynamic pa = ViewModel.PaInfo;
                    if (pa == null) return;
                    txt_time.Text = string.Format("建于{0:yyyy-MM-dd HH:mm:ss} || 最后修改{1:yyyy-MM-dd HH:mm:ss}", pa.CreateTime, pa.UpdateTime);
                });
            }
            base.OnNavigatedTo(e);
            {
                txt_nonfile.Text = "";
                this.btn_Pwd.Tag = "0";
                btn_Pwd_click(this.btn_Pwd, null);
            }
        }

        private void btn_Pwd_click(object sender, RoutedEventArgs e)
        {
            var btn = this.btn_Pwd;
            if (ObjectHelper.AreEquals(btn.Tag, "1"))
            {
                btn.Tag = "0";
                this.txt_Pwd.Text = btn.DataContext as string;
            }
            else if (ObjectHelper.AreEquals(btn.Tag, "0"))
            {
                btn.Tag = "1";
                this.txt_Pwd.Text = "********";
            }
        }

        private async void btn_filedownload_click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            dynamic fileModel = btn.DataContext;
            var file = await ((fileModel.GetFile() as Task<byte[]>) ?? Task.FromResult<byte[]>(null));
            if (file == null || file.Length == 0)
            {
                txt_nonfile.Text = "没附件";
                return;
            }
            txt_nonfile.Text = "";
            var fd = new SaveFileDialog();
            fd.Filter = string.Format("{0}|*{0}", string.IsNullOrEmpty(fileModel.FileExtname) ? ".*" : fileModel.FileExtname);
            fd.AddExtension = true;
            if (fd.ShowDialog() == true)
            {
                using (var fs = fd.OpenFile())
                {
                    await fs.WriteAsync(file, 0, file.Length);
                }
            }
        }

    }
}
