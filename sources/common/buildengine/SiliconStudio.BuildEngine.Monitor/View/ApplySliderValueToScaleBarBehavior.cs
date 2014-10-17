// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

using SiliconStudio.Presentation.Controls;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    class ApplySliderValueToScaleBarBehavior : Behavior<Slider>
    {
        public ScaleBar ScaleBar { get { return (ScaleBar)GetValue(ScaleBarProperty); } set { SetValue(ScaleBarProperty, value); } }
        public bool IsAutoFitActive { get { return (bool)GetValue(AutoFitActiveProperty); } set { SetValue(AutoFitActiveProperty, value); } }

        public static readonly DependencyProperty ScaleBarProperty = DependencyProperty.Register("ScaleBar", typeof(ScaleBar), typeof(ApplySliderValueToScaleBarBehavior), new PropertyMetadata(null, ScaleBarChanged));
        public static readonly DependencyProperty AutoFitActiveProperty = DependencyProperty.Register("IsAutoFitActive", typeof(bool), typeof(ApplySliderValueToScaleBarBehavior), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ValueChanged += ValueChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ValueChanged -= ValueChanged;
            base.OnDetaching();
        }

        private static void ScaleBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (ApplySliderValueToScaleBarBehavior)d;

            if (e.OldValue != null)
            {
                ((ScaleBar)e.OldValue).ScaleChanged -= behavior.ScaleChanged;

            }
            if (e.NewValue != null)
            {
                ((ScaleBar)e.NewValue).ScaleChanged += behavior.ScaleChanged;
            }
        }

        private bool isUpdateInProgress;

        private void ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            if (isUpdateInProgress)
                return;

            if (IsAutoFitActive)
                SetCurrentValue(AutoFitActiveProperty, false);

            var logMin = Math.Log(ScaleBar.MinimumUnitsPerTick);
            var logMax = Math.Log(ScaleBar.MaximumUnitsPerTick);
            var scale = (logMax - logMin) / (AssociatedObject.Maximum -  AssociatedObject.Minimum);
            var result = Math.Exp(logMin + scale * (AssociatedObject.Value - AssociatedObject.Minimum));

            isUpdateInProgress = true;
            ScaleBar.UnitsPerTick = result;
            isUpdateInProgress = false;
        }

        private void ScaleChanged(object sender, RoutedDependencyPropertyChangedEventArgs routedDependencyPropertyChangedEventArgs)
        {
            if (isUpdateInProgress)
                return;

            var logMin = Math.Log(ScaleBar.MinimumUnitsPerTick);
            var logMax = Math.Log(ScaleBar.MaximumUnitsPerTick);
            var scale = (logMax - logMin) / (AssociatedObject.Maximum - AssociatedObject.Minimum);
            var result = (Math.Log(ScaleBar.UnitsPerTick) - logMin + scale * ScaleBar.MinimumUnitsPerTick) / scale;

            isUpdateInProgress = true;
            AssociatedObject.Value = result;
            isUpdateInProgress = false;
        }
    }
}
