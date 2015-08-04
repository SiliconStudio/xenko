// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D
using System;
using System.Runtime.InteropServices;
using System.Security;

using SharpDX.Win32;
using SharpDX.Mathematics.Interop;

namespace SiliconStudio.Paradox.Games
{
    /// <summary>
    /// Internal class to interact with Native Message
    /// </summary>
    internal partial class Win32Native
    {
        [DllImport("user32.dll", EntryPoint = "GetClientRect")]
        public static extern bool GetClientRect(IntPtr hWnd, out RawRectangle lpRect);

        [DllImport("user32.dll", EntryPoint = "PeekMessage")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int PeekMessage(
            out NativeMessage lpMsg,
            IntPtr hWnd,
            int wMsgFilterMin,
            int wMsgFilterMax,
            int wRemoveMsg);

        [DllImport("user32.dll", EntryPoint = "GetMessage")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int GetMessage(
            out NativeMessage lpMsg,
            IntPtr hWnd,
            int wMsgFilterMin,
            int wMsgFilterMax);

        [DllImport("user32.dll", EntryPoint = "TranslateMessage")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int TranslateMessage(ref NativeMessage lpMsg);

        [DllImport("user32.dll", EntryPoint = "DispatchMessage")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int DispatchMessage(ref NativeMessage lpMsg);
    }
}
#endif