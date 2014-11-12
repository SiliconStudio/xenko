// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace SiliconStudio.Presentation.Extensions
{
    public static class NativeHelper
    {
        #region Methods

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int mCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hmonitor,   [In, Out] MONITORINFO monitorInfo);

        #endregion Methods

        #region Structures

        // ReSharper disable InconsistentNaming

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MONITORINFO
        {
            public int cbSize = sizeof(int) * 10;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X, Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        #endregion Structures

        #region Constants

        public const int GWL_STYLE = unchecked((int)0xFFFFFFF0);

        public const int MONITOR_DEFAULTTONULL = unchecked(0x00000000);
        public const int MONITOR_DEFAULTTOPRIMARY = unchecked(0x00000001);
        public const int MONITOR_DEFAULTTONEAREST = unchecked(0x00000002);

        // Window Styles - http://msdn.microsoft.com/en-us/library/windows/desktop/ms632600%28v=vs.85%29.aspx
        public const int WS_BORDER = unchecked(0x00800000);
        public const int WS_CAPTION = unchecked(0x00C00000);
        public const int WS_CHILD = unchecked(0x40000000);
        public const int WS_CHILDWINDOW = unchecked(0x40000000);
        public const int WS_CLIPCHILDREN = unchecked(0x02000000);
        public const int WS_CLIPSIBLINGS = unchecked(0x04000000);
        public const int WS_DISABLED = unchecked(0x08000000);
        public const int WS_DLGFRAME = unchecked(0x00400000);
        public const int WS_GROUP = unchecked(0x00020000);
        public const int WS_HSCROLL = unchecked(0x00100000);
        public const int WS_ICONIC = unchecked(0x20000000);
        public const int WS_MAXIMIZE = unchecked(0x01000000);
        public const int WS_MAXIMIZEBOX = unchecked(0x00010000);
        public const int WS_MINIMIZE = unchecked(0x20000000);
        public const int WS_MINIMIZEBOX = unchecked(0x00020000);
        public const int WS_OVERLAPPED = unchecked(0x00000000);
        public const int WS_OVERLAPPEDWINDOW = unchecked(WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        public const int WS_POPUP = unchecked((int)0x80000000);
        public const int WS_POPUPWINDOW = unchecked(WS_POPUP | WS_BORDER | WS_SYSMENU);
        public const int WS_SIZEBOX = unchecked(0x00040000);
        public const int WS_SYSMENU = unchecked(0x00080000);
        public const int WS_TABSTOP = unchecked(0x00010000);
        public const int WS_THICKFRAME = unchecked(0x00040000);
        public const int WS_TILED = unchecked(0x00000000);
        public const int WS_TILEDWINDOW = unchecked(WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        public const int WS_VISIBLE = unchecked(0x10000000);
        public const int WS_VSCROLL = unchecked(0x00200000);

        public const int SW_HIDE = unchecked(0x00000000);
        // ReSharper restore InconsistentNaming

        #endregion Constants

        public static bool SetCursorPos(Point pt)
        {
            return SetCursorPos((int)pt.X, (int)pt.Y);
        }
    }
}
#endif