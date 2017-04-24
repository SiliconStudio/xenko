// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Graphics
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
