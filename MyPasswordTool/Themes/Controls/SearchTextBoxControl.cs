using SilverEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MyPasswordTool
{
    //public class SearchTextEventArgs : EventArgs
    //{ 
    //}

	public class SearchTextBoxControl : TextBox
	{
		public SearchTextBoxControl()
		{
            base.DefaultStyleKey = typeof(SearchTextBoxControl);
		}

        public event EventHandler<object> OnSearch;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var btn = base.GetTemplateChild("button") as Button;
            btn.Click += (_, __) => 
            {
                OnSearch?.Invoke(this, EventArgs.Empty);
            };
            this.MouseEnter += (_, __) => 
            {
                VisualStateManager.GoToState(this, "path_MouseOver", false);
            };
            this.MouseLeave += (_, __) =>
            {
                VisualStateManager.GoToState(this, "path_Normal", false);
            };
        }
	}
}