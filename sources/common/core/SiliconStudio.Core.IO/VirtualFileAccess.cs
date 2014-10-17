// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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