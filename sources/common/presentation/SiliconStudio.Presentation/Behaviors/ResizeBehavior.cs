using System.Windows;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Behaviors
{
    public sealed class ResizeBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// Identifies the <see cref="SizeRatio"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SizeRatioProperty = DependencyProperty.Register("SizeRatio", typeof(Size), typeof(ResizeBehavior));

        public Size SizeRatio
        {
            get { return (Size)GetValue(SizeRatioProperty); }
            set { SetValue(SizeRatioProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.SizeChanged += SizeChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SizeChanged -= SizeChanged;
        }

        private void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var height = AssociatedObject.MaxHeight;
            //var width = AssociatedObject.MaxWidth;

            //if (e.HeightChanged)
            //{
            //    width = e.NewSize.Height*SizeRatio.Width/SizeRatio.Height;
            //}
            //if (e.WidthChanged)
            //{
            //    height = e.NewSize.Width*SizeRatio.Height/SizeRatio.Width;
            //}
            //AssociatedObject.MaxHeight = height;
            //AssociatedObject.MaxWidth = width;
        }
    }
}
