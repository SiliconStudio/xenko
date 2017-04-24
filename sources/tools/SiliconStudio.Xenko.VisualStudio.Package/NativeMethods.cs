// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Runtime.InteropServices;

namespace SiliconStudio.Xenko.VisualStudio
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
