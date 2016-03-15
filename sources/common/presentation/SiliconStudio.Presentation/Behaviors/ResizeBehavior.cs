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

            // Measure the required size
            AssociatedObject.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var desiredSize = AssociatedObject.DesiredSize;
            var surface = desiredSize.Height*desiredSize.Width;

            var height = Math.Round(Math.Sqrt(SizeRatio.Height*surface/SizeRatio.Width));
            height = MathUtil.Clamp(height, AssociatedObject.MinHeight, AssociatedObject.MaxHeight);

            var width = Math.Round(SizeRatio.Width*height/SizeRatio.Height);
            AssociatedObject.Width = MathUtil.Clamp(width, AssociatedObject.MinWidth, AssociatedObject.MaxWidth);
            if (width < AssociatedObject.MinWidth)
            {
                // Keep default value for height
                return;
            }
            AssociatedObject.Height = height;
        }

        private bool IsSizeRatioInValid()
        {
            return SizeRatio.IsEmpty
                || double.IsNaN(SizeRatio.Width) || double.IsInfinity(SizeRatio.Width) || SizeRatio.Width < 1
                || double.IsNaN(SizeRatio.Height) || double.IsInfinity(SizeRatio.Height) || SizeRatio.Height < 1;
        }
    }
}
