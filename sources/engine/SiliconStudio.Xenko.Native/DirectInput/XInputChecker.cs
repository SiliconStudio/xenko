// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.InteropServices;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Native.DirectInput
{
    /// <summary>
    /// Finds out if a device is an XInputDevice
    /// </summary>
    public static class XInputChecker
    {
        static XInputChecker()
        {
            NativeInvoke.PreLoad();
        }

        /// <summary>
        /// Check if device represented by <paramref name="guid"/> is indeed an XInput device.
        /// </summary>
        /// <param name="guid">Guid of device to check.</param>
        /// <returns>True if XInput device.</returns>
        [DllImport(NativeInvoke.Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsXInputDevice(ref Guid guid);
    }
}
