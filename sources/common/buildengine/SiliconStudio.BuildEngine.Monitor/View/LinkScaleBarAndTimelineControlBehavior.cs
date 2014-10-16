// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Interactivity;

using Org.Dna.Aurora.UIFramework.Wpf.Timeline;

using SiliconStudio.Presentation.Controls;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    class LinkScaleBarAndTimelineControlBehavior : Behavior<ScaleBar>
    {
        public static readonly DependencyProperty TimelineControlProperty = DependencyProperty.Register("TimelineControl", typeof(TimelineControl), typeof(LinkScaleBarAndTimelineControlBehavior));

        public TimelineControl TimelineControl
        {
            get { return (TimelineControl)GetValue(TimelineControlProperty); }
            set { SetValue(TimelineControlProperty, value); }
        }

        private double StartUnitToScrollOffset(double startUnit)
        {
            var ticksCount = startUnit * TimeSpan.TicksPerSecond;
            double offset = ticksCount / TimelineControl.TickTimeSpan.Ticks;
            return offset;
        }

        private void ScaleChanged(object sender, RoutedDependencyPropertyChangedEventArgs e)
        {
            double start = AssociatedObject.StartUnit;
            if (TimelineControl != null)
            {
                TimelineControl.TickTimeSpan = TimeSpan.FromTicks(Math.Max(1, (long)(TimeSpan.TicksPerSecond * AssociatedObject.UnitsPerTick / AssociatedObject.PixelsPerTick)));
                TimelineControl.ScrollViewer.ScrollToHorizontalOffset(StartUnitToScrollOffset(start));
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ScaleChanged += ScaleChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ScaleChanged -= ScaleChanged;
            base.OnDetaching();
        }

    }
}
