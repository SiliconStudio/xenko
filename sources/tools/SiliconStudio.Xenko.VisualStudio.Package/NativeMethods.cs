// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Runtime.InteropServices;

namespace SiliconStudio.Paradox.VisualStudio
{
    internal static class NativeMethods
    {
        public static int ThrowOnFailure(int hr)
        {
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return hr;
        }
    }
}