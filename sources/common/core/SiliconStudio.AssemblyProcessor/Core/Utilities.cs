// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SiliconStudio.Core
{
    internal static class Utilities
    {
        // ***************************************************************
        // NOTE: This file is shared with the AssemblyProcessor.
        // If this file is modified, the AssemblyProcessor has to be
        // recompiled separately. See build\Xenko-AssemblyProcessor.sln
        // ***************************************************************

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr CopyMemory(IntPtr dest, IntPtr src, ulong sizeInBytesToCopy);

        public static void CopyMemory(IntPtr dest, IntPtr src, int sizeInBytesToCopy)
        {
            CopyMemory(dest, src, (ulong)sizeInBytesToCopy);
        }
    }
}
