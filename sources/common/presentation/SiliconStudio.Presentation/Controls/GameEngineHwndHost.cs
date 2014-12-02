// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    /// <summary>
    /// An implementation of the <see cref="HwndHost"/> class adapted to embed a game engine viewport in a WPF application.
    /// </summary>
    public class GameEngineHwndHost : HwndHost
    {
        private readonly IntPtr childHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameEngineHwndHost"/> class.
        /// </summary>
        /// <param name="childHandle">The hwnd of the child (hosted) window.</param>
        public GameEngineHwndHost(IntPtr childHandle)
        {
            this.childHandle = childHandle;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            NativeHelper.SetParent(childHandle, IntPtr.Zero);
            NativeHelper.DestroyWindow(hwnd.Handle);
        }

        /// <summary>
        /// Forwards a message that comes from the hosted window to the WPF window. This method can be used for example to forward keyboard events.
        /// </summary>
        /// <param name="hwnd">The hwnd of the hosted window.</param>
        /// <param name="msg">The message identifier.</param>
        /// <param name="wParam">The word parameter of the message.</param>
        /// <param name="lParam">The long parameter of the message.</param>
        public void ForwardMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            var parent = NativeHelper.GetParent(hwnd);
            NativeHelper.PostMessage(parent, msg, wParam, lParam);
        }
    }
}
