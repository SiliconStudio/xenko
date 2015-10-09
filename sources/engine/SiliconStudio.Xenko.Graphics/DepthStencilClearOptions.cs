// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Specifies the buffer to use when calling Clear.
    /// </summary>
    [Flags]
    public enum DepthStencilClearOptions
    {
        /// <summary>
        /// A depth buffer.
        /// </summary>
        DepthBuffer = 1,
        /// <summary>
        /// A stencil buffer.
        /// </summary>
        Stencil = 2,
    }
}
