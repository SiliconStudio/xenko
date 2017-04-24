// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Graphics
{
    [Flags]
    public enum TextureFlags
    {
        /// <summary>
        /// No option.
        /// </summary>
        None = 0,

        /// <summary>
        /// A texture usable as a ShaderResourceView.
        /// </summary>
        ShaderResource = 1,

        /// <summary>
        /// A texture usable as render target.
        /// </summary>
        RenderTarget = 2,

        /// <summary>
        /// A texture usable as an unordered access buffer.
        /// </summary>
        UnorderedAccess = 4,

        /// <summary>
        /// A texture usable as a depth stencil buffer.
        /// </summary>
        DepthStencil = 8,

        /// <summary>
        /// A texture usable as a readonly depth stencil buffer.
        /// </summary>
        DepthStencilReadOnly = 8 + Texture.DepthStencilReadOnlyFlags,
    }
}
