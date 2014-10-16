// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Interactivity;

using SiliconStudio.Presentation.Controls;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    class AutofitTimelineBehavior : Behavior<ScaleBar>
    {
        public static readonly DependencyProperty MinimumDateProperty = DependencyProperty.Register("MinimumDate", typeof(DateTime), typeof(AutofitTimelineBehavior), new PropertyMetadata(default(DateTime), FitValueChanged));

        public DateTime MinimumDate
        {
            get { return (DateTime)GetValue(MinimumDateProperty); }
            set { SetValue(MinimumDateProperty, value); }
        }

        public static readonly DependencyProperty MaximumDateProperty = DependencyProperty.Register("MaximumDate", typeof(DateTime), typeof(AutofitTimelineBehavior), new PropertyMetadata(default(DateTime), FitValueChanged));

        public DateTime MaximumDate
        {
            get { return (DateTime)GetValue(MaximumDateProperty); }
            set { SetValue(MaximumDateProperty, value); }
        }

        public static readonly DependencyProperty TickTimeSpanProperty = DependencyProperty.Register("TickTimeSpan", typeof(TimeSpan), typeof(AutofitTimelineBehavior), new PropertyMetadata(default(TimeSpan)));

        public TimeSpan TickTimeSpan
        {
            get { return (TimeSpan)GetValue(TickTimeSpanProperty); }
            set { throw new NotSupportedException("This property is read-only, it has a setter only for binding support (due to http://connect.microsoft.com/VisualStudio/feedback/details/540833/onewaytosource-binding-from-a-readonly-dependency-property)"); }
        }

        public static readonly DependencyProperty ActiveProperty = DependencyProperty.Register("Active", typeof(bool), typeof(AutofitTimelineBehavior), new PropertyMetadata(default(bool), FitValueChanged));

        public bool Active
        {
            get { return (bool)GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SizeChanged += SizeChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SizeChanged -= SizeChanged;
            base.OnDetaching();
        }

        private void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FitValue();
        }

        private static void FitValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (AutofitTimelineBehavior)d;
            behavior.FitValue();
        }

        private void FitValue()
        {
            if (Active && AssociatedObject != null)
            {
                AssociatedObject.StartUnit = 0;

                // We don't want to resize too often because it's time consuming.
                TimeSpan timeSpan = MaximumDate - MinimumDate;

                // So rescale when we reach 95% of the space...
                double width = AssociatedObject.ActualWidth * 0.95;
                var timeSpanTicks = Math.Ceiling(timeSpan.Ticks / width);
                if (timeSpanTicks > TickTimeSpan.Ticks)
                {
                    // ...to 70% of the space.
                    width = AssociatedObject.ActualWidth * 0.7;
                    timeSpanTicks = Math.Max(0, Math.Ceiling(timeSpan.Ticks / width));
                    AssociatedObject.UnitsPerTick = AssociatedObject.PixelsPerTick * timeSpanTicks / TimeSpan.TicksPerSecond;
                }

                // Also, when the user has just reactivated the AutoFit option, we might be under the 70%...
                width = AssociatedObject.ActualWidth * 0.7;
                timeSpanTicks = Math.Ceiling(timeSpan.Ticks / width);
                if (timeSpanTicks < TickTimeSpan.Ticks)
                {
                    width = AssociatedObject.ActualWidth * 0.7;
                    timeSpanTicks = Math.Max(0, Math.Ceiling(timeSpan.Ticks / width));
                    AssociatedObject.UnitsPerTick = AssociatedObject.PixelsPerTick * timeSpanTicks / TimeSpan.TicksPerSecond;
                }
            }
        }
    }
}
