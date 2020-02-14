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
    public partial class PaTagsWin : UserControl, IView<PainfoTagsWinViewModel>
    {
        public PaTagsWin()
        {
            InitializeComponent();
        }

        public PainfoTagsWinViewModel ViewModel
        {
            get { return this.DataContext as PainfoTagsWinViewModel; }
        }
    }
}
