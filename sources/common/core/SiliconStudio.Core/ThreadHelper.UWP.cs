// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_UWP
using System;

// Some missing delegates/exceptions when compiling against WinRT/WinPhone 8.1
namespace SiliconStudio.Core
{
    public delegate void ThreadStart();

    public delegate void ParameterizedThreadStart(object obj);

    class ThreadAbortException : Exception
    {
        
    }
}
#endif
