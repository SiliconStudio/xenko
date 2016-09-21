using System;
using System.Windows;
using System.Windows.Interop;
using SiliconStudio.Presentation.Interop;

namespace SiliconStudio.Presentation.Extensions
{
    /// <summary>
    /// Extension helpers for the <see cref="System.Windows.Window"/> class.
    /// </summary>
    public static class WindowHelper
    {
        /// <summary>
        /// Moves the <see cref="window"/> to the center of the given <see cref="area"/>.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="area">The aera.</param>
        public static void CenterToArea(this Window window, Rect area)
        {
            if (area == Rect.Empty) return;

            window.Left = Math.Abs(area.Width - window.Width) / 2 + area.Left;
            window.Top = Math.Abs(area.Height - window.Height) / 2 + area.Top;
        }

        /// <summary>
        /// Moves the <see cref="window"/> to the center of the current screen's work area.
        /// </summary>
        /// <param name="window">The window.</param>
        public static void CenterToWorkArea(this Window window)
        {
            var workArea = GetWorkArea(window);
            if (workArea == Rect.Empty) return;

            window.CenterToArea(workArea);
        }

        /// <summary>
        /// Gets the available work area for this <see cref="window"/> on the current screen.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        public static Rect GetWorkArea(this Window window)
        {
            var monitor = GetMonitorInfo(new WindowInteropHelper(window).Handle);
            if (monitor == null) return Rect.Empty;

            var area = monitor.rcWork;
            return new Rect(area.Left, area.Top, area.Right - area.Left, area.Bottom - area.Top);
        }

        /// <summary>
        /// Gets the size of the screen monitor for this <see cref="window"/>.
        /// </summary>
        /// <param name="window">The window.</param>>
        /// <returns></returns>
        public static Rect GetScreenSize(this Window window)
        {
            var monitor = GetMonitorInfo(new WindowInteropHelper(window).Handle);
            if (monitor == null) return Rect.Empty;

            var area = monitor.rcMonitor;
            return new Rect(area.Left, area.Top, area.Right - area.Left, area.Bottom - area.Top);
        }

        /// <summary>
        /// Moves and resize the <see cref="window"/> to make it fill the whole given <see cref="area"/>.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="area">The area.</param>
        public static void FillArea(this Window window, Rect area)
        {
            window.Width = area.Width;
            window.Height = area.Height;
            window.Left = area.Left;
            window.Top = area.Top;
        }

        /// <summary>
        /// Moves and resize the <see cref="window"/> to make it fill all the available current screen's work area.
        /// </summary>
        /// <param name="window">The window.</param>
        public static void FillWorkArea(this Window window)
        {
            var workArea = GetWorkArea(window);
            if (workArea == Rect.Empty) return;

            window.FillArea(workArea);
        }

        #region Internals
        internal static NativeHelper.MONITORINFO GetMonitorInfo(IntPtr hWnd)
        {
            var monitor = NativeHelper.MonitorFromWindow(hWnd, NativeHelper.MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new NativeHelper.MONITORINFO();
                NativeHelper.GetMonitorInfo(monitor, monitorInfo);
                return monitorInfo;
            }

            return null;
        }
        #endregion // Internals
    }
}
