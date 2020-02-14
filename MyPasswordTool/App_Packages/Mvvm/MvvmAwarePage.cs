using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
    public class MvvmAwarePage : MyPage
    {
        protected override void OnNavigatedTo(MyNavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            (DataContext as IMvvmAware)?.Activate(e.Parameter);
        }

        protected override void OnNavigatedFrom(MyNavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            (DataContext as IMvvmAware)?.Deactivate(e.Parameter);
        }
    }
}