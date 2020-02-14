using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MyPasswordTool
{
    [ContentProperty(nameof(LazyloadTemplate))]
    public partial class LazyloadContent : Border
    {
        public LazyloadContent()
        {
            this.DefaultStyleKey = typeof(LazyloadContent);
        }

        public UIElement Content
        {
            get => base.Child;
            set => base.Child = value;
        }

        public static readonly DependencyProperty LazyloadTemplateProperty =
            DependencyProperty.Register("LazyloadTemplate", typeof(DataTemplate), typeof(LazyloadContent),
                new PropertyMetadata(null));

        public DataTemplate LazyloadTemplate
        {
            get => (DataTemplate)GetValue(LazyloadTemplateProperty);
            set => SetValue(LazyloadTemplateProperty, value);
        }

        public static readonly DependencyProperty CanLoadProperty =
            DependencyProperty.Register("CanLoad", typeof(bool), typeof(LazyloadContent),
                new PropertyMetadata(false, OnCanLoadPropertyChanged));

        private static void OnCanLoadPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                ((LazyloadContent)d).OnLoadContent();
        }

        public bool CanLoad
        {
            get => (bool)GetValue(CanLoadProperty);
            set => SetValue(CanLoadProperty, value);
        }

        private void OnLoadContent()
        {
            if (this.Content != null) return;
            this.Content = LazyloadTemplate.LoadContent() as FrameworkElement;
        }
    }
}
