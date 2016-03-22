// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

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
        /// When attached to a <see cref="Window"/> that have the <see cref="Window.WindowStyle"/> value set to <see cref="WindowStyle.None"/>, prevent the window to expand over the taskbar when maximized.
        /// </summary>
        public static DependencyProperty KeepTaskbarWhenMaximizedProperty = DependencyProperty.RegisterAttached("KeepTaskbarWhenMaximized", typeof(bool), typeof(BehaviorProperties), new PropertyMetadata(false, KeepTaskbarWhenMaximizedChanged));

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

        /// <summary>
        /// Gets the current value of the <see cref="KeepTaskbarWhenMaximizedProperty"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <returns>The value of the <see cref="KeepTaskbarWhenMaximizedProperty"/> dependency property.</returns>
        public static bool GetKeepTaskbarWhenMaximized(DependencyObject target)
        {
            return (bool)target.GetValue(KeepTaskbarWhenMaximizedProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="KeepTaskbarWhenMaximizedProperty"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="value">The value to set.</param>
        public static void SetKeepTaskbarWhenMaximized(DependencyObject target, bool value)
        {
            target.SetValue(KeepTaskbarWhenMaximizedProperty, value);
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
                var frameworkElement = d as FrameworkElement;
                if (frameworkElement != null && !frameworkElement.IsLoaded)
                {
                    // Let's delay the behavior till the scroll viewer is loaded.
                    frameworkElement.Loaded += (sender, args) =>
                    {
                        var dependencyObject = (DependencyObject)sender;
                        var loadedScrollViewer = dependencyObject.FindVisualChildOfType<ScrollViewer>();
                        if (loadedScrollViewer != null)
                            typeof(ScrollViewer).GetProperty("HandlesMouseWheelScrolling", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(loadedScrollViewer, e.NewValue);
                    };
                }
            }
        }

        private static void KeepTaskbarWhenMaximizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window == null)
                return;

            if (window.IsLoaded)
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                var source = HwndSource.FromHwnd(hwnd);
                source?.AddHook(
                    (IntPtr h, int msg, IntPtr wparam, IntPtr lparam, ref bool handled) => WindowProc(window, h, msg, wparam, lparam, ref handled));
            }
            else
            {
                window.SourceInitialized += (sender, arg) =>
                {
                    var hwnd = new WindowInteropHelper(window).Handle;
                    var source = HwndSource.FromHwnd(hwnd);
                    source?.AddHook(
                        (IntPtr h, int msg, IntPtr wparam, IntPtr lparam, ref bool handled) => WindowProc(window, h, msg, wparam, lparam, ref handled));
                };
            }
        }

        private static IntPtr WindowProc(Window window, IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            switch (msg)
            {
                /* WM_GETMINMAXINFO */
                case 0x0024:
                    var monitorInfo = WindowHelper.GetMonitorInfo(hwnd);
                    if (monitorInfo == null)
                        break;

                    var mmi = (NativeHelper.MINMAXINFO)Marshal.PtrToStructure(lparam, typeof(NativeHelper.MINMAXINFO));
                    var rcWorkArea = monitorInfo.rcWork;
                    var rcMonitorArea = monitorInfo.rcMonitor;

                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    // Get maximum width and height from WPF
                    var maxWidth = double.IsInfinity(window.MaxWidth) ? int.MaxValue : (int)window.MaxWidth;
                    var maxHeight = double.IsInfinity(window.MaxHeight) ? int.MaxValue : (int)window.MaxHeight;
                    mmi.ptMaxSize.X = Math.Min(maxWidth, Math.Abs(rcWorkArea.Right - rcWorkArea.Left));
                    mmi.ptMaxSize.Y = Math.Min(maxHeight, Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top));
                    mmi.ptMaxTrackSize.X = mmi.ptMaxSize.X;
                    mmi.ptMaxTrackSize.Y = mmi.ptMaxSize.Y;

                    Marshal.StructureToPtr(mmi, lparam, true);
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
