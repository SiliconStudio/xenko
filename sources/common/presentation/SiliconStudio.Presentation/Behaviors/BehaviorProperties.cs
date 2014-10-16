// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// This static class contains attached dependency properties that can be used as behavior to add or change features of controls.
    /// </summary>
    public static class BehaviorProperties
    {
        /// <summary>
        /// When attached to a <see cref="ScrollViewer"/> or a control that contains a <see cref="ScrollViewer"/>, this property allows to control whether the scroll viewer should handle scrolling with the mouse wheel.
        /// </summary>
        public static DependencyProperty HandlesMouseWheelScrollingProperty = DependencyProperty.RegisterAttached("HandlesMouseWheelScrolling", typeof(bool), typeof(BehaviorProperties), new PropertyMetadata(true, HandlesMouseWheelScrollingChanged));

        /// <summary>
        /// Gets the current value of the <see cref="HandlesMouseWheelScrollingProperty"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <returns>The value of the <see cref="HandlesMouseWheelScrollingProperty"/> dependency property.</returns>
        public static bool GetHandlesMouseWheelScrolling(DependencyObject target)
        {
            return (bool)target.GetValue(HandlesMouseWheelScrollingProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="HandlesMouseWheelScrollingProperty"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="value">The value to set.</param>
        public static void SetHandlesMouseWheelScrolling(DependencyObject target, bool value)
        {
            target.SetValue(HandlesMouseWheelScrollingProperty, value);
        }

        private static void HandlesMouseWheelScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer ?? d.FindVisualChildOfType<ScrollViewer>();

            if (scrollViewer != null)
            {
                // Yet another internal property that should be public.
                typeof(ScrollViewer).GetProperty("HandlesMouseWheelScrolling", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(scrollViewer, e.NewValue);
            }
            else
            {
                // The framework element is not loaded yet and thus the ScrollViewer is not reachable.
                var loadedHandler = new RoutedEventHandler((sender, args) =>
                    {
                        var dependencyObject = (DependencyObject)sender;
                        var loadedScrollViewer = dependencyObject.FindVisualChildOfType<ScrollViewer>();
                        if (loadedScrollViewer != null)
                            typeof(ScrollViewer).GetProperty("HandlesMouseWheelScrolling", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(loadedScrollViewer, e.NewValue);
                    });

                var frameworkElement = d as FrameworkElement;
                if (frameworkElement != null && !frameworkElement.IsLoaded)
                {
                    // Let's delay the behavior till the scroll viewer is loaded.
                    frameworkElement.Loaded += loadedHandler;
                }
            }
        }
    }
}
