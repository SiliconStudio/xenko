// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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