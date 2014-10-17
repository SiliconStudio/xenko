// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// Describes the different type of streams.
    /// </summary>
    [Flags]
    public enum StreamFlags
    {
        /// <summary>
        /// Returns the default underlying stream without any alterations. Can be a seek-able stream or not depending on the file.
        /// </summary>
        None,

        /// <summary>
        /// A stream in which we can seek
        /// </summary>
        Seekable,
    }
}