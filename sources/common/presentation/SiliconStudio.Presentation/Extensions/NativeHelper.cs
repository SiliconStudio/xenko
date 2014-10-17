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

        // ReSharper disable InconsistentNaming
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

        public const int GWL_STYLE = unchecked((int)0xFFFFFFF0);

        public const int WS_CAPTION = unchecked(0x00C00000);
        public const int WS_THICKFRAME = unchecked(0x00040000);
        public const int WS_CHILD = unchecked(0x40000000);

        public const int SW_HIDE = unchecked(0x00000000);
        // ReSharper restore InconsistentNaming

        public static bool SetCursorPos(Point pt)
        {
            return SetCursorPos((int)pt.X, (int)pt.Y);
        }
    }
}
#endif