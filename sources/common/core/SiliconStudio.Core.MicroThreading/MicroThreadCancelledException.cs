// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.MicroThreading
{
    /// <summary>
    /// Exception thrown when a MicroThread is cancelled (usally due to live scripting reloading).
    /// </summary>
    public class MicroThreadCancelledException : Exception
    {
         
    }
}