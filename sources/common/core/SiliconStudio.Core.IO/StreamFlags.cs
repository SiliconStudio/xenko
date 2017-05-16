// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
