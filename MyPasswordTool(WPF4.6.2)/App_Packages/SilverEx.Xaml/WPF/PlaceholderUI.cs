using System.Windows;

namespace SilverEx.Xaml
{
    public class PlaceholderUI : FrameworkElement
    {
        public PlaceholderUI()
        {
            this.Visibility = Visibility.Collapsed;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return new Size(0, 0);
        }
    }
}
