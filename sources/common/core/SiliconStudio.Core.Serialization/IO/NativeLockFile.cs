#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SiliconStudio.Core.IO
{
    class NativeLockFile
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct OVERLAPPED
        {
            public uint internalLow;
            public uint internalHigh;
            public uint offsetLow;
            public uint offsetHigh;
            public IntPtr hEvent;
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool LockFileEx(SafeFileHandle handle, uint flags, uint reserved, uint countLow, uint countHigh, ref OVERLAPPED overlapped);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool UnlockFileEx(SafeFileHandle handle, uint reserved, uint countLow, uint countHigh, ref OVERLAPPED overlapped);

        public const uint LOCKFILE_FAIL_IMMEDIATELY = 0x00000001;
        public const uint LOCKFILE_EXCLUSIVE_LOCK = 0x00000002;
    }
}
#endif