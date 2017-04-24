// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// File access equivalent of <see cref="System.IO.FileAccess"/>.
    /// </summary>
    [Flags]
    public enum VirtualFileAccess : uint
    {
        /// <summary>
        /// Read access.
        /// </summary>
        Read = 1,

        /// <summary>
        /// Write access.
        /// </summary>
        Write = 2,

        /// <summary>
        /// Read/Write Access,
        /// </summary>
        ReadWrite = Read | Write,
    }
}
