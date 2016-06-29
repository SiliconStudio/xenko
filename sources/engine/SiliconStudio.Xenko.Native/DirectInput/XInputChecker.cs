using System;
using System.Runtime.InteropServices;

namespace SiliconStudio.Xenko.Native.DirectInput
{
    public static class XInputChecker
    {
        /// <summary>
        /// Check if device represented by <paramref name="guid"/> is indeed an XInput device.
        /// </summary>
        /// <param name="guid">Guid of device to check.</param>
        /// <returns>True if XInput device.</returns>
        [DllImport(NativeInvoke.Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsXInputDevice(ref Guid guid);
    }
}
