// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SiliconStudio.BuildEngine.Monitor.ViewModel;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    /// <summary>
    /// Interaction logic for BuildTimelineUserControl.xaml
    /// </summary>
    public partial class BuildTimelineUserControl
    {
        public DateTime? MinimumDate { get { return (DateTime?)GetValue(MinimumDateProperty); } set { SetValue(MinimumDateProperty, value); } }

        public DateTime? MaximumDate { get { return (DateTime?)GetValue(MaximumDateProperty); } set { SetValue(MaximumDateProperty, value); } }

        public IEnumerable ItemsSource { get { return (IEnumerable)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }

        public bool AutoFitActive { get { return (bool)GetValue(AutoFitActiveProperty); } set { SetValue(AutoFitActiveProperty, value); } }

        public static readonly DependencyProperty MinimumDateProperty = DependencyProperty.Register("MinimumDate", typeof(DateTime?), typeof(BuildTimelineUserControl));
        public static readonly DependencyProperty MaximumDateProperty = DependencyProperty.Register("MaximumDate", typeof(DateTime?), typeof(BuildTimelineUserControl));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(BuildTimelineUserControl));
        public static readonly DependencyProperty AutoFitActiveProperty = DependencyProperty.Register("AutoFitActive", typeof(bool), typeof(BuildTimelineUserControl), new FrameworkPropertyMetadata(true));

        public const double MouseWheelZoomCoeficient = 1.1;

        public BuildTimelineUserControl()
        {
            InitializeComponent();
            TimelineControl.TickTimeSpan = new TimeSpan(0, 0, 0, 0, 1);
        }

        private void TimelineControlPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (AutoFitActive)
                SetCurrentValue(AutoFitActiveProperty, false);

            double coeficient = e.Delta >= 0.0 ? MouseWheelZoomCoeficient : 1.0 / MouseWheelZoomCoeficient;
            Point pos = e.GetPosition(this);

            ScaleBar.ZoomAtPosition(pos.X, coeficient, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

            e.Handled = true;
        }

        private void MicrothreadJobViewModel_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var ui = (FrameworkElement)sender;
            var viewmodel = ui.DataContext as MicrothreadJobViewModel;

            if (viewmodel != null)
            {
                foreach (var mtjViewModel in TimelineControl.ItemsSource)
                {
                    ((MicrothreadJobViewModel)mtjViewModel).UpdatingHighlightedMicrothreadJob();
                }

                MicrothreadJobViewModel.HighlightedMicrothreadJob = viewmodel.MicrothreadId;

                foreach (var mtjViewModel in TimelineControl.ItemsSource)
                {
                    ((MicrothreadJobViewModel)mtjViewModel).UpdatedHighlightedMicrothreadJob();
                }
            }
        }

        private void MicrothreadJobViewModel_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var ui = (FrameworkElement)sender;
            var viewmodel = ui.DataContext as MicrothreadJobViewModel;

            if (viewmodel != null)
            {
                foreach (var mtjViewModel in TimelineControl.ItemsSource)
                {
                    ((MicrothreadJobViewModel)mtjViewModel).UpdatingHighlightedMicrothreadJob();
                }

                MicrothreadJobViewModel.HighlightedMicrothreadJob = -1;

                foreach (var mtjViewModel in TimelineControl.ItemsSource)
                {
                    ((MicrothreadJobViewModel)mtjViewModel).UpdatedHighlightedMicrothreadJob();
                }
            }
        }

        private void TimelineControl_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScaleBar.StartUnit = TimelineControl.ScrollViewer.HorizontalOffset * TimelineControl.TickTimeSpan.TotalSeconds;
        }
    }
}
