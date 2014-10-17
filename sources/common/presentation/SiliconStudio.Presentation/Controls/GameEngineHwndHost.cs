// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    public class GameEngineHwndHost : HwndHost
    {
        private readonly IntPtr childHandle;
        private Rect previousBoundingBox;

        public GameEngineHwndHost(IntPtr childHandle)
        {
            this.childHandle = childHandle;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            int style = NativeHelper.GetWindowLong(childHandle, NativeHelper.GWL_STYLE);
            // Removes Caption bar and the sizing border
            // Must be a child window to be hosted
            style |= NativeHelper.WS_CHILD;

            NativeHelper.SetWindowLong(childHandle, NativeHelper.GWL_STYLE, style);
            NativeHelper.ShowWindow(childHandle, NativeHelper.SW_HIDE);

            NativeHelper.SetParent(childHandle, hwndParent.Handle);

            var hwnd = new HandleRef(this, childHandle);
            return hwnd;
        }

        protected override void OnWindowPositionChanged(Rect rcBoundingBox)
        {
            if (previousBoundingBox != rcBoundingBox)
            {
                base.OnWindowPositionChanged(rcBoundingBox);
                previousBoundingBox = rcBoundingBox;
            }
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            NativeHelper.SetParent(childHandle, IntPtr.Zero);
        }
    }
}
