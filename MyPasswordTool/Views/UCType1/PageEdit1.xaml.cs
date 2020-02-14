using Common;
using Microsoft.Win32;
using MyPasswordTool.Models;
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
    //[ExportAsView("edit1")]
    public partial class PageEdit1 : MvvmAwarePage, IView<Edit1ViewModel>
    {
        public PageEdit1()
        {
            InitializeComponent();
            txt_file.DataContextChanged += (_, e) => fm_changed(e.NewValue);
            txt_file.Text = "[0/(最大不能超过3M)]";
        }

        public Edit1ViewModel ViewModel { get { return this.DataContext as Edit1ViewModel; } }
        public IStore Store { get; } = IoC.Resolve<IStore>();

        private void fm_changed(dynamic fm)
        {
            if (fm == null) return;
            var f = fm.FileSize;
            object s = "";
            if (f == null || f == 0) s = "0";
            else if (f < 1024) s = f + "B";
            else if (f / 1024.0 < 1024) s = string.Format("{0:f2}K", f / 1024.0);
            else s = string.Format("{0:f2}M", f / (1024.0 * 1024.0));
            txt_file.Text = string.Format("[{0}/(最大不能超过3M)]", s);
        }

        private void btn_tags_click(object sender, RoutedEventArgs e) 
        {
            var btn = sender as Control;
            btn.IsEnabled = false;
            Store.Dispatch(new NavigationMessage()
            {
                Type = Consts.NS.ChildWin,
                ViewKey = Consts.ViewKey.patags_win,
                Parameter = new
                {
                    GetTags = new Func<PaTag[]>(() => btn.DataContext.AsTo<PaTag[]>()),
                    SetTags = new Action<PaTag[]>(tags => 
                    {
                        btn.DataContext = tags;
                        btn.IsEnabled = true;
                    }),
                },
            });
        }

        private async void btn_ico_click(object sender, RoutedEventArgs e) 
        {
            await ThreadUtil.VoidTask;
            var btn = this.btn_ico;
            var fd = new OpenFileDialog();
            fd.Filter = Consts.IcoFilter;
            fd.Multiselect = false;
            fd.AddExtension = true;
            if (fd.ShowDialog() == true)
            {
                using (var fs = fd.OpenFile())
                {
                    if (fs.Length > 80 * 1024)
                    {
                        MessageBox.Show("图片不能大于80k");
                        return;
                    }
                    var by = new byte[fs.Length];
                    await fs.ReadAsync(by, 0, by.Length);
                    btn.DataContext = by;
                }
            }
        }

        private async void btn_filedownload_click(object sender, RoutedEventArgs e)
        {
            await ThreadUtil.VoidTask;
            var btn = sender as Button;
            if (btn == null) return;
            dynamic fileModel = btn.DataContext;
            var file = fileModel.File as byte[];
            if (fileModel.IsUpdated == false && file == null)
            {
                file = await ((fileModel.GetFile() as Task<byte[]>) ?? Task.FromResult<byte[]>(null));
            }
            if (file == null || file.Length == 0) return;
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

        private async void btn_fileupload_click(object sender, RoutedEventArgs e)
        {
            await ThreadUtil.VoidTask;
            var btn = sender as Button;
            if (btn == null) return;
            dynamic fileModel = btn.DataContext;
            var fd = new OpenFileDialog();
            fd.Filter = "*.*|*.*";
            fd.Multiselect = false;
            fd.AddExtension = true;
            if (fd.ShowDialog() == true)
            {
                using (var fs = fd.OpenFile())
                {
                    if (fs.Length > 3 * 1024 * 1024)
                    {
                        MessageBox.Show("附件不能大于3M");
                        return;
                    }
                    var by = new byte[fs.Length];
                    await fs.ReadAsync(by, 0, by.Length);
                    fileModel.IsUpdated = true;
                    fileModel.File = by;
                    fileModel.FileSize = by.Length;
                    var ei = fd.FileName.LastIndexOf(".");
                    fileModel.FileExtname = ei > -1 ? fd.FileName.Substring(ei) : null;
                    fm_changed(fileModel);
                }
            }
        }

        private void btn_fileclean_click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            dynamic fileModel = btn.DataContext;
            fileModel.IsUpdated = true;
            fileModel.File = null;
            fileModel.FileSize = null;
            fileModel.FileExtname = null;
            fm_changed(fileModel);
        }

        private void _del_ico_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PaInfo["ICO"] = null;
        }
    }
}
