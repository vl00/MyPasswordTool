using SilverEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyPasswordTool
{
    public partial class ChildWindow : UserControl
    {
        public ChildWindow()
        {
            InitializeComponent();
            this.Loaded += ChildWindow_Loaded;
            this.Unloaded += ChildWindow_Unloaded;
        }

        private Button cbtn;
        private Grid root;
        private IEnumerable<VisualState> vs;

        internal static Panel Host { get; set; }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ChildWindow), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public event EventHandler<CancelEventArgs> Closing;

        public void Show()
        {
            if (!this.IsElementLoaded()) ChildWindow.Host.Children.Add(this);
        }

        public void Close()
        {
            cbtn_Click(null, null);
        }

        private async void cbtn_Click(object sender, RoutedEventArgs e)
        {
            var h = Closing;
            if (h != null)
            {
                var _e = new CancelEventArgs();
                h(this, _e);
                if (_e.Cancel) return;
            }
            var sb = vs.First(s => s.Name == "IsVisible_false").Storyboard;
            await EventUtil.FromEventAsync<EventArgs>(
                eh => sb.Completed += new EventHandler(eh),
                eh => sb.Completed -= new EventHandler(eh),
                () => sb.Begin(root));
            this.Visibility = Visibility.Collapsed;
            ChildWindow.Host.Children.Remove(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            cbtn = this.GetTemplateChild("CloseButton") as Button;
            cbtn.Click += cbtn_Click;
            root = this.GetTemplateChild("Root") as Grid;
            vs = VisualStates.TryGetVisualStateGroup(this, "VisualStateGroup").States.OfType<VisualState>();
        }

        private void ChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ChildWindow_Loaded;
            vs.First(s => s.Name == "IsVisible_true").Storyboard.Begin(root);
        }

        private void ChildWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ChildWindow_Unloaded;
            cbtn.Click -= cbtn_Click;
            this.Closing = null;
        }
    }
}
