using System;
using System.Windows;
using System.Windows.Interactivity;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Behaviors
{
    public sealed class ResizeBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// Identifies the <see cref="SizeRatio"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SizeRatioProperty =
            DependencyProperty.Register(nameof(SizeRatio), typeof(Size), typeof(ResizeBehavior));

        public Size SizeRatio { get { return (Size)GetValue(SizeRatioProperty); } set { SetValue(SizeRatioProperty, value); } }

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
            if (IsSizeRatioInValid() || !e.HeightChanged || !e.WidthChanged)
                return;
            
            var height = double.PositiveInfinity;
            var width = double.PositiveInfinity;
            // Measure the required size
            AssociatedObject.Measure(new Size(width, height));
            var desiredSize = AssociatedObject.DesiredSize;

            var surface = desiredSize.Height*desiredSize.Width;
            height = Math.Round(Math.Sqrt(SizeRatio.Height*surface/SizeRatio.Width));
            width = Math.Round(SizeRatio.Width*height/SizeRatio.Height);

            if (width < AssociatedObject.MinWidth)
            {
                // Keep default size
                AssociatedObject.Width = AssociatedObject.MinWidth;
                return;
            }
            AssociatedObject.Height = MathUtil.Clamp(height, AssociatedObject.MinHeight, AssociatedObject.MaxHeight);
            AssociatedObject.Width = MathUtil.Clamp(width, AssociatedObject.MinWidth, AssociatedObject.MaxWidth);
        }

        private bool IsSizeRatioInValid()
        {
            return SizeRatio.IsEmpty
                || double.IsNaN(SizeRatio.Width) || double.IsInfinity(SizeRatio.Width) || SizeRatio.Width < 1
                || double.IsNaN(SizeRatio.Height) || double.IsInfinity(SizeRatio.Height) || SizeRatio.Height < 1;
        }
    }
}
