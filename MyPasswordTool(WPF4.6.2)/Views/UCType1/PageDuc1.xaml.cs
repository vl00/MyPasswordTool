using Common;
using Microsoft.Win32;
using MyPasswordTool.ViewModels;
using SilverEx;
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
    public partial class PageDuc1 : MvvmAwarePage, IView<Duc1ViewModel>
    {
        public PageDuc1()
        {
            InitializeComponent();
        }

        private bool is_noted;

        public Duc1ViewModel ViewModel
        {
            get { return this.DataContext as Duc1ViewModel; }
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
                is_noted = false;
                btn_Note_click(this.btn_Note, null);
            }
        }

        private void btn_Note_click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (is_noted)
            {
                is_noted = false;
                this.txt_Note.Text = btn.DataContext as string;
            }
            else 
            {
                is_noted = true;
                this.txt_Note.Text = "********";
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
